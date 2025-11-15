using UnityEngine;

/// <summary>
/// 玩家動畫控制器
/// 連接 PlayerMove 和 CharacterStateMachine，根據玩家狀態自動切換動畫
/// </summary>
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(CharacterStateMachine))]
public class PlayerAnimationController : MonoBehaviour
{
    private PlayerMove playerMove;
    private CharacterStateMachine stateMachine;
    
    [Header("狀態檢測設定")]
    [Tooltip("移動速度閾值，超過此值視為移動")]
    public float moveSpeedThreshold = 0.1f;
    
    [Tooltip("檢測地面狀態的延遲（避免頻繁切換）")]
    public float groundCheckDelay = 0.1f;
    
    [Tooltip("狀態切換的最小時間間隔（避免頻繁切換）")]
    public float stateChangeCooldown = 0.1f;
    
    [Tooltip("使用移動方向而不是速度來判斷是否移動（更穩定）")]
    public bool useMoveDirectionForWalk = true;
    
    private float lastGroundCheckTime;
    private bool wasGrounded;
    private float lastStateChangeTime;
    private CharacterStateMachine.StateType lastStateType;
    private bool wasCharging = false; // 記錄上一幀是否在蓄力
    
    [Header("轉向動畫設定")]
    [Tooltip("轉向角度閾值（度），超過此角度才觸發轉向動畫")]
    public float turnAngleThreshold = 45f;
    
    [Tooltip("轉向動畫持續時間（秒）")]
    public float turnAnimationDuration = 0.3f;
    
    private Vector3 lastForwardDirection; // 上一幀的朝向
    private float turnAnimationTimer = 0f; // 轉向動畫計時器
    private bool isInTurnAnimation = false; // 是否正在播放轉向動畫
    private CharacterStateMachine.StateType turnDirection = CharacterStateMachine.StateType.Walk; // 轉向方向
    
    void Start()
    {
        // 獲取組件
        playerMove = GetComponent<PlayerMove>();
        stateMachine = GetComponent<CharacterStateMachine>();
        
        if (playerMove == null)
        {
            Debug.LogError("PlayerAnimationController: 找不到 PlayerMove 組件！");
        }
        
        if (stateMachine == null)
        {
            Debug.LogError("PlayerAnimationController: 找不到 CharacterStateMachine 組件！");
        }
        
        // 初始化狀態
        wasGrounded = true;
        lastStateChangeTime = 0f;
        lastStateType = CharacterStateMachine.StateType.Idle;
        lastForwardDirection = transform.forward;
    }
    
    void Update()
    {
        if (playerMove == null || stateMachine == null)
        {
            return;
        }
        
        // 獲取玩家狀態信息
        Vector3 moveDirection = playerMove.GetMoveDirection();
        Vector3 velocity = playerMove.GetCurrentVelocity();
        bool isGrounded = IsPlayerGrounded();
        bool isCharging = playerMove.IsCharging();
        
        // 計算水平移動速度
        float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        
        // 更新蓄力動畫參數
        if (stateMachine != null && stateMachine.animator != null)
        {
            stateMachine.SetAnimatorBool("IsCharging", isCharging);
        }
        
        // 處理轉向動畫（只有在非蓄力狀態下才處理）
        if (!isCharging)
        {
            HandleTurnAnimation(moveDirection, isGrounded);
        }
        
        // 根據玩家狀態切換動畫狀態
        UpdateAnimationState(moveDirection, horizontalSpeed, isGrounded, isCharging);
        
        // 更新地面狀態記錄
        if (Time.time - lastGroundCheckTime > groundCheckDelay)
        {
            // 檢測落地事件
            if (!wasGrounded && isGrounded)
            {
                OnLanding();
            }
            wasGrounded = isGrounded;
            lastGroundCheckTime = Time.time;
        }
        
        // 更新朝向記錄
        if (moveDirection.magnitude > 0.1f)
        {
            lastForwardDirection = moveDirection.normalized;
        }
    }
    
    /// <summary>
    /// 檢查玩家是否在地面上
    /// </summary>
    private bool IsPlayerGrounded()
    {
        if (playerMove != null)
        {
            return playerMove.IsGrounded();
        }
        return true;
    }
    
    /// <summary>
    /// 根據玩家狀態更新動畫狀態
    /// </summary>
    private void UpdateAnimationState(Vector3 moveDirection, float horizontalSpeed, bool isGrounded, bool isCharging)
    {
        // 如果正在蓄力跳躍，完全鎖定當前狀態，不進行任何切換或計算
        if (isCharging)
        {
            // 記錄蓄力狀態，確保在蓄力期間不會切換動畫
            wasCharging = true;
            // 完全返回，不進行任何狀態計算或切換
            return;
        }
        
        // 如果剛從蓄力狀態退出，重置標記
        if (wasCharging && !isCharging)
        {
            wasCharging = false;
            // 重置狀態切換時間，允許立即切換到正確的狀態
            lastStateChangeTime = 0f;
        }
        
        CharacterStateMachine.StateType targetState = stateMachine.GetCurrentStateType();
        
        // 獲取當前速度（用於判斷跳躍/下落）
        Vector3 currentVelocity = playerMove.GetCurrentVelocity();
        
        // 如果正在播放轉向動畫，不進行狀態切換（讓轉向動畫完成）
        if (isInTurnAnimation)
        {
            return; // 轉向動畫期間不切換狀態
        }
        
        // 檢查是否正在播放落地動畫
        if (stateMachine != null && stateMachine.animator != null)
        {
            AnimatorStateInfo stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("骨架|land"))
            {
                // 如果落地動畫還沒播放完成，不進行狀態切換
                if (stateInfo.normalizedTime < 0.95f)
                {
                    return;
                }
            }
        }
        
        // 根據是否在地面來決定狀態
        if (!isGrounded)
        {
            // 在空中
            if (currentVelocity.y > 0.1f)
            {
                // 上升中，使用跳躍動畫
                targetState = CharacterStateMachine.StateType.Jump;
            }
            else
            {
                // 下降中，使用下落動畫
                targetState = CharacterStateMachine.StateType.Fall;
            }
        }
        else
        {
            // 在地面上
            bool isMoving = false;
            
            if (useMoveDirectionForWalk)
            {
                // 使用移動方向判斷（更穩定，避免速度波動導致抽搐）
                // 如果有輸入方向，就視為移動
                isMoving = moveDirection.magnitude > 0.1f;
            }
            else
            {
                // 使用速度判斷（傳統方式）
                isMoving = horizontalSpeed > moveSpeedThreshold;
            }
            
            if (isMoving)
            {
                // 正在移動
                targetState = CharacterStateMachine.StateType.Walk;
            }
            else
            {
                // 靜止
                targetState = CharacterStateMachine.StateType.Idle;
            }
        }
        
        // 檢查是否需要切換狀態
        CharacterStateMachine.StateType currentStateType = stateMachine.GetCurrentStateType();
        
        // 如果狀態需要改變
        if (currentStateType != targetState)
        {
            // 檢查冷卻時間（避免頻繁切換）
            float timeSinceLastChange = Time.time - lastStateChangeTime;
            
            // 對於某些狀態（如空中狀態），允許立即切換
            bool isAirState = targetState == CharacterStateMachine.StateType.Jump || 
                             targetState == CharacterStateMachine.StateType.Fall;
            bool isAirToGround = (currentStateType == CharacterStateMachine.StateType.Jump || 
                                 currentStateType == CharacterStateMachine.StateType.Fall) &&
                                 (targetState == CharacterStateMachine.StateType.Idle || 
                                 targetState == CharacterStateMachine.StateType.Walk);
            
            // 如果超過冷卻時間，或者是空中狀態切換，或者是從空中到地面，則允許切換
            // 但必須確保不在蓄力狀態（這個檢查已經在上面做了，這裡是雙重保險）
            if ((timeSinceLastChange >= stateChangeCooldown || isAirState || isAirToGround) && !isCharging)
            {
                stateMachine.ChangeState(targetState);
                lastStateChangeTime = Time.time;
                lastStateType = targetState;
            }
        }
        else
        {
            // 狀態沒有改變，更新記錄
            lastStateType = currentStateType;
        }
    }
    
    /// <summary>
    /// 處理轉向動畫
    /// </summary>
    private void HandleTurnAnimation(Vector3 moveDirection, bool isGrounded)
    {
        // 如果正在播放轉向動畫，更新計時器
        if (isInTurnAnimation)
        {
            turnAnimationTimer -= Time.deltaTime;
            if (turnAnimationTimer <= 0f)
            {
                isInTurnAnimation = false;
            }
            return; // 轉向動畫播放期間不檢測新的轉向
        }
        
        // 只有在移動且在地面上時才檢測轉向
        if (!isGrounded || moveDirection.magnitude < 0.1f)
        {
            return;
        }
        
        // 計算當前朝向和上一幀朝向的角度差
        Vector3 currentForward = moveDirection.normalized;
        float angle = Vector3.SignedAngle(lastForwardDirection, currentForward, Vector3.up);
        
        // 如果角度變化超過閾值，觸發轉向動畫
        if (Mathf.Abs(angle) > turnAngleThreshold)
        {
            // 確定轉向方向
            if (angle > 0)
            {
                // 右轉
                turnDirection = CharacterStateMachine.StateType.TurnRight;
            }
            else
            {
                // 左轉
                turnDirection = CharacterStateMachine.StateType.TurnLeft;
            }
            
            // 觸發轉向動畫
            isInTurnAnimation = true;
            turnAnimationTimer = turnAnimationDuration;
            stateMachine.ChangeState(turnDirection);
        }
    }
    
    /// <summary>
    /// 落地事件處理
    /// </summary>
    private void OnLanding()
    {
        // 如果從空中落地，觸發落地動畫
        CharacterStateMachine.StateType currentState = stateMachine.GetCurrentStateType();
        if (currentState == CharacterStateMachine.StateType.Jump || 
            currentState == CharacterStateMachine.StateType.Fall)
        {
            // 使用 Animator 直接播放落地動畫
            if (stateMachine.animator != null)
            {
                // 直接播放落地動畫，落地動畫播放完成後會自動根據移動狀態切換
                stateMachine.PlayAnimation("骨架|land", 0.15f);
            }
        }
    }
}
