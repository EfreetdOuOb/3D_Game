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
    
    private float lastGroundCheckTime;
    private bool wasGrounded;
    
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
        
        // 根據玩家狀態切換動畫狀態
        UpdateAnimationState(moveDirection, horizontalSpeed, isGrounded, isCharging);
        
        // 更新地面狀態記錄
        if (Time.time - lastGroundCheckTime > groundCheckDelay)
        {
            wasGrounded = isGrounded;
            lastGroundCheckTime = Time.time;
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
        CharacterStateMachine.StateType targetState = stateMachine.GetCurrentStateType();
        
        // 如果正在蓄力跳躍，保持當前狀態（或可以添加蓄力動畫）
        if (isCharging)
        {
            // 可以選擇保持當前狀態或添加新的蓄力狀態
            return;
        }
        
        // 獲取當前速度（用於判斷跳躍/下落）
        Vector3 currentVelocity = playerMove.GetCurrentVelocity();
        
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
            if (horizontalSpeed > moveSpeedThreshold)
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
        
        // 如果狀態需要改變，則切換狀態
        if (stateMachine.GetCurrentStateType() != targetState)
        {
            stateMachine.ChangeState(targetState);
        }
    }
}
