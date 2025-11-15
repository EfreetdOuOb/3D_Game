using System;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private Vector3 moveDirection;
    private BaseState current_state;

    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    
    [Header("模型朝向設定")]
    [Tooltip("模型朝向偏移（度），用於修正模型的forward方向。例如：如果模型面向右側，設置為90；如果面向後方，設置為180")]
    public float modelForwardOffset = 0f;
    
    [Header("移動慣性設定")]
    [Tooltip("移動加速度（0 = 無慣性，立即響應；值越大慣性越強）")]
    public float moveAcceleration = 20f;
    [Tooltip("停止時的減速度（0 = 立即停止；值越大停止越快）")]
    public float moveDeceleration = 25f;
    [Tooltip("是否啟用移動慣性（關閉則立即響應輸入）")]
    public bool enableMoveInertia = true;
    
    [Header("蓄力跳躍設定")]
    public float minJumpForce = 5f;      // 最小跳躍力度
    public float maxJumpForce = 15f;     // 最大跳躍力度
    public float chargeRate = 20f;       // 蓄力速度
    public float maxChargeTime = 1.5f;   // 最大蓄力時間
    
    [Header("跳躍緩衝設定")]
    public float coyoteTime = 0.15f;     // 土狼時間（離地後仍可跳躍的時間）
    
    [Header("地面檢測")]
    public Transform groundCheck;
    public float groundCheckHeight = 0.1f;
    public float groundCheckWidth = 0.4f; // capsule較窄，調整檢測寬度
    public LayerMask groundMask;
    
    [Header("攝像機")]
    public Camera playerCamera;
    
    
    
    [Header("蓄力狀態")]
    private bool isCharging = false;
    private float currentChargeTime = 0f;
    private float currentJumpForce = 0f;
    
    [Header("跳躍緩衝狀態")]
    private float lastGroundedTime = 0f;     // 最後一次在地面的時間
    private bool wasGrounded = false;        // 上一幀是否在地面
    
    [Header("跳躍啟用（支援地板debuff）")]
    public bool canJump = true;
    
    [Header("空中下墜調整")]
    public bool enableExtraGravity = true;
    public float fallGravityMultiplier = 2.2f;
    public float lowJumpGravityMultiplier = 1.8f;
    public bool enableHangTimeCut = false;
    public float maxHangTime = 0.35f;
    public float hangTimeDownForce = 25f;
    public bool enableFallSpeedClamp = false;
    public float maxFallSpeed = 35f;
    
    [Header("高能模式設定")]
    public float maxEnergy = 100f;              // 最大能量
    public float energyConsumptionRate = 10f;   // 每秒消耗的能量
    public float minEnergyThreshold = 5f;       // 自動關閉的最小能量閾值
    public float energyRechargeRate = 0f;       // 非高能模式下的能量恢復速度（可選）
    
    [Header("高能模式能力增強")]
    public float highEnergyJumpMultiplier = 1.5f;   // 跳躍力倍數
    public float highEnergySpeedMultiplier = 1.3f;  // 移動速度倍數
    public float highEnergyRotationMultiplier = 1.2f; // 旋轉速度倍數
    
    [Header("高能模式狀態")]
    private bool isHighEnergyMode = false;      // 是否處於高能模式
    private float currentEnergy = 100f;         // 當前能量
    
    [Header("空中狀態紀錄")]
    private bool isJumpHeld = false;
    private bool hasJumpedSinceGrounded = false;
    private float jumpStartTime = -1f;
    
    // 獲取蓄力進度（0-1之間），供UI使用
    public float GetChargeProgress()
    {
        return Mathf.Clamp01(currentChargeTime / maxChargeTime);
    }
    
    // 獲取當前跳躍力度，供UI使用
    public float GetCurrentJumpForce()
    {
        return currentJumpForce;
    }
    
    // 是否正在蓄力，供UI使用
    public bool IsCharging()
    {
        return isCharging;
    }
    
    // 獲取玩家移動方向，供跳墊使用
    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }
    
    // 獲取玩家當前速度，供跳墊使用
    public Vector3 GetCurrentVelocity()
    {
        return rb.linearVelocity;
    }
    
    // 獲取玩家是否在地面上，供動畫系統使用
    public bool IsGrounded()
    {
        return isGrounded;
    }
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        
        SetCurrentState(new Idle(this));
        // 初始化能量
        currentEnergy = maxEnergy;
        
        // 如果沒有攝像機，嘗試找到主攝像機
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        // 如果沒有地面檢測點，創建一個（針對capsule調整位置）
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0); // capsule底部偏移
            groundCheck = groundCheckObj.transform;
        }
        
        // 設定物理材質，針對capsule優化（減少滑動，適合滾動移動）
        PhysicsMaterial playerPhysicsMaterial = new PhysicsMaterial("PlayerPhysics");
        playerPhysicsMaterial.dynamicFriction = 0;  // 增加摩擦力，適合capsule
        playerPhysicsMaterial.staticFriction = 0;   // 增加靜摩擦力
        playerPhysicsMaterial.bounciness = 0.1f;       // 稍微增加彈性，模擬capsule特性
        playerPhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Multiply; // 使用最大摩擦力
        playerPhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Multiply;
        
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            playerCollider.material = playerPhysicsMaterial;
        }
    }
    
    void Update()
    {
        current_state.Update();
        // 檢測是否在地面上
        CheckGrounded();
        
        // 更新土狼時間
        UpdateCoyoteTime();
        
        // 處理高能模式
        HandleHighEnergyMode();
        
        // 獲取輸入
        HandleInput();
        
        // 蓄力跳躍（輸入檢測在 Update 中）
        HandleChargeJump();
    }
    
    void FixedUpdate()
    {
        current_state.FixedUpdate();
        // 物理相關操作在 FixedUpdate 中執行，確保 FPS 無關
        // 移動角色
        MovePlayer();
        
        // 應用空中物理修正
        ApplyAirPhysicsModifiers();
    }
    
    public void CheckGrounded()
    {
        // 使用射線檢測地面，針對capsule調整檢測距離
        Vector3 rayStart = groundCheck.position + Vector3.up * 0.05f; // capsule需要更小的偏移
        isGrounded = Physics.Raycast(rayStart, Vector3.down, 1.1f, groundMask); // 調整檢測距離
        
        // 額外的射線檢測，針對capsule形狀優化
        Vector3[] rayPositions = {
            groundCheck.position + new Vector3(groundCheckWidth * 0.7f, 0.05f, 0), // 縮小檢測範圍
            groundCheck.position + new Vector3(-groundCheckWidth * 0.7f, 0.05f, 0),
            groundCheck.position + new Vector3(0, 0.05f, groundCheckWidth * 0.7f),
            groundCheck.position + new Vector3(0, 0.05f, -groundCheckWidth * 0.7f)
        };
        
        foreach (Vector3 pos in rayPositions)
        {
            if (Physics.Raycast(pos, Vector3.down, 1.1f, groundMask))
            {
                isGrounded = true;
                break;
            }
        }
    }
    
    public void HandleInput()
    {
        // 獲取WASD輸入
        float horizontal = Input.GetAxis("Horizontal"); // A/D 或 左/右箭頭
        float vertical = Input.GetAxis("Vertical");     // W/S 或 上/下箭頭
        
        // 計算移動方向（相對於攝像機）
        if (playerCamera != null)
        {
            // 獲取攝像機的右方向（水平移動）
            Vector3 cameraRight = playerCamera.transform.right;
            // 獲取攝像機的前方向（垂直移動），但忽略Y軸
            Vector3 cameraForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            
            // 計算相對於攝像機的移動方向
            moveDirection = (cameraRight * horizontal + cameraForward * vertical).normalized;
        }
        else
        {
            // 如果沒有攝像機，使用世界座標
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }
    }
    
    public void MovePlayer()
    {
        // 計算移動速度（高能模式下增強）
        float currentMoveSpeed = moveSpeed;
        float currentRotationSpeed = rotationSpeed;
        
        if (isHighEnergyMode)
        {
            currentMoveSpeed *= highEnergySpeedMultiplier;
            currentRotationSpeed *= highEnergyRotationMultiplier;
        }
        
        // 計算目標速度
        Vector3 targetVelocity = moveDirection * currentMoveSpeed;
        
        // 獲取當前水平速度（X和Z軸）
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // 根據是否啟用慣性來計算新速度
        Vector3 newHorizontalVelocity;
        
        if (enableMoveInertia)
        {
            // 使用平滑加速度/減速度（使用 Time.fixedDeltaTime 確保 FPS 無關）
            if (moveDirection.magnitude > 0.1f) // 有輸入
            {
                // 加速到目標速度
                float acceleration = moveAcceleration > 0 ? moveAcceleration : float.MaxValue;
                newHorizontalVelocity = Vector3.MoveTowards(
                    currentHorizontalVelocity, 
                    targetVelocity, 
                    acceleration * Time.fixedDeltaTime
                );
            }
            else // 沒有輸入，減速
            {
                // 減速到零
                float deceleration = moveDeceleration > 0 ? moveDeceleration : float.MaxValue;
                newHorizontalVelocity = Vector3.MoveTowards(
                    currentHorizontalVelocity, 
                    Vector3.zero, 
                    deceleration * Time.fixedDeltaTime
                );
            }
        }
        else
        {
            // 無慣性模式：立即響應輸入
            newHorizontalVelocity = targetVelocity;
        }
        
        // 只影響X和Z軸，保持重力效果
        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
        
        // 讓角色面向移動方向（旋轉在 Update 中處理更平滑，但這裡也可以）
        if (moveDirection != Vector3.zero)
        {
            // 應用模型朝向偏移
            Quaternion offsetRotation = Quaternion.Euler(0, modelForwardOffset, 0);
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * offsetRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.fixedDeltaTime);
        }
    }
    public bool IsMoving()
    {
        return moveDirection.magnitude > 0.1f;
    }
    
    public void HandleChargeJump()
    {
        bool jumpDown = Input.GetButtonDown("Jump");
        bool jumpHeld = Input.GetButton("Jump");
        bool jumpUp = Input.GetButtonUp("Jump");
        isJumpHeld = jumpHeld;
        
        // 開始蓄力（按下跳躍鍵）
        if (jumpDown)
        {
            if (isCharging && CanJump())
            {
                // 如果已經預蓄力且現在可以跳躍，立即釋放
                ReleaseJump();
            }
            else if (!isCharging)
            {
                if (CanJump())
                {
                    // 在地面或土狼時間內，立即開始蓄力
                    StartCharging();
                }
                else
                {
                    // 在空中，開始預蓄力
                    StartPreCharging();
                }
            }
        }
        
        // 持續蓄力（按住跳躍鍵且正在蓄力）
        if (jumpHeld && isCharging)
        {
            ContinueCharging();
        }
        
        // 釋放跳躍（鬆開跳躍鍵）
        if (jumpUp && isCharging)
        {
            ReleaseJump();
        }
        
        // 如果不在蓄力狀態，重置蓄力時間
        if (!isCharging)
        {
            currentChargeTime = 0f;
        }
    }
    
    void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
        currentJumpForce = minJumpForce;
        Debug.Log("開始蓄力跳躍 - 按住空格鍵蓄力，鬆開釋放");
    }
    
    void StartPreCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
        currentJumpForce = minJumpForce;
        Debug.Log("空中預蓄力開始 - 著地後可立即跳躍");
    }
    
    void ContinueCharging()
    {
        currentChargeTime += Time.deltaTime;
        
        // 計算蓄力進度（0到1之間）
        float chargeProgress = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        
        // 使用平滑曲線計算跳躍力度
        currentJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargeProgress);
        
        // 蓄力達到最大值後停止增加，但不自動釋放
        if (currentChargeTime >= maxChargeTime)
        {
            currentChargeTime = maxChargeTime;
            currentJumpForce = maxJumpForce;
        }
    }
    
    void ReleaseJump()
    {
        if (CanJump())
        {
            // 計算跳躍力度（高能模式下增強）
            float jumpForce = currentJumpForce;
            if (isHighEnergyMode)
            {
                jumpForce *= highEnergyJumpMultiplier;
            }
            
            // 重置Y軸速度，確保跳躍高度一致
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            // 使用 Impulse 模式確保 FPS 無關（瞬間施加力）
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            hasJumpedSinceGrounded = true;
            jumpStartTime = Time.time; // 修正：使用 Time.time 而不是 Time.deltaTime
            
            float chargeProgress = GetChargeProgress();
            bool usingCoyoteTime = !isGrounded && (Time.time - lastGroundedTime <= coyoteTime);
            string jumpType = usingCoyoteTime ? " [土狼時間]" : " [地面跳躍]";
            string energyMode = isHighEnergyMode ? " [高能模式]" : "";
            Debug.Log($"跳躍力度: {jumpForce:F1} (蓄力時間: {currentChargeTime:F2}s, 進度: {chargeProgress:P0}){jumpType}{energyMode}");
            
            // 重置蓄力狀態
            isCharging = false;
            currentChargeTime = 0f;
            currentJumpForce = minJumpForce;
        }
        else
        {
            // 在空中釋放，保存蓄力狀態，等待手動釋放
            float chargeProgress = GetChargeProgress();
            Debug.Log($"空中預蓄力完成 - 力度: {currentJumpForce:F1} (蓄力時間: {currentChargeTime:F2}s, 進度: {chargeProgress:P0}) - 著地後按空格鍵釋放");
            // 保持蓄力狀態，不重置
        }
    }
    
    public void UpdateCoyoteTime()
    {
        // 記錄地面狀態變化
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            hasJumpedSinceGrounded = false;
            jumpStartTime = -1f;
        }
        
        wasGrounded = isGrounded;
    }
    
    bool CanJump()
    {
        // 新增判斷canJump欄位
        if (!canJump) return false;
        // 檢查是否在地面上
        if (isGrounded)
        {
            return true;
        }
        
        // 檢查土狼時間（離地後仍可跳躍的時間）
        float timeSinceGrounded = Time.time - lastGroundedTime;
        if (timeSinceGrounded <= coyoteTime)
        {
            return true;
        }
        
        return false;
    }
    
    public void ApplyAirPhysicsModifiers()
    {
        if (rb == null)
        {
            return;
        }
        
        // 使用更嚴格的條件判斷是否在空中（避免邊界情況）
        if (isGrounded)
        {
            return;
        }
        
        Vector3 velocity = rb.linearVelocity;
        
        // 使用 ForceMode.Acceleration 並在 FixedUpdate 中調用，確保 FPS 無關
        // Acceleration 模式會自動考慮 Time.fixedDeltaTime
        if (enableExtraGravity)
        {
            // 改進條件判斷：使用更寬鬆的閾值，確保下墜重力穩定應用
            if (velocity.y <= 0.1f) // 改為 <= 0.1f，包括接近 0 的情況
            {
                // 下落時或接近下落時增加重力（確保下墜效果穩定）
                float multiplier = Mathf.Max(1f, fallGravityMultiplier);
                rb.AddForce(Vector3.up * Physics.gravity.y * (multiplier - 1f), ForceMode.Acceleration);
            }
            else if (!isJumpHeld && velocity.y > 0.1f)
            {
                // 上升時（未按住跳躍鍵）增加重力，實現低跳躍
                float multiplier = Mathf.Max(1f, lowJumpGravityMultiplier);
                rb.AddForce(Vector3.up * Physics.gravity.y * (multiplier - 1f), ForceMode.Acceleration);
            }
        }
        
        if (enableHangTimeCut && hasJumpedSinceGrounded && velocity.y > 0f)
        {
            if (jumpStartTime >= 0f && Time.time - jumpStartTime >= maxHangTime)
            {
                // 懸停時間過長後施加向下的力
                rb.AddForce(Vector3.down * hangTimeDownForce, ForceMode.Acceleration);
            }
        }
        
        if (enableFallSpeedClamp)
        {
            // 限制最大下落速度
            float maxDownSpeed = -Mathf.Abs(maxFallSpeed);
            if (velocity.y < maxDownSpeed)
            {
                velocity.y = maxDownSpeed;
                rb.linearVelocity = velocity;
            }
        }
    }
    
    public void HandleHighEnergyMode()
    {
        // 檢測右鍵輸入（Fire2 對應滑鼠右鍵或 Alt 鍵）
        if (Input.GetButtonDown("Fire2"))
        {
            ToggleHighEnergyMode();
        }
        
        // 如果處於高能模式，持續消耗能量
        if (isHighEnergyMode)
        {
            currentEnergy -= energyConsumptionRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            
            // 檢查能量是否低於閾值，自動關閉
            if (currentEnergy <= minEnergyThreshold)
            {
                SetHighEnergyMode(false);
                Debug.Log("能量不足，自動關閉高能模式");
            }
        }
        else if (energyRechargeRate > 0f)
        {
            // 非高能模式下可以恢復能量（可選）
            currentEnergy += energyRechargeRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }
    }
    
    void ToggleHighEnergyMode()
    {
        // 如果當前能量不足以開啟高能模式，則不允許開啟
        if (!isHighEnergyMode && currentEnergy <= minEnergyThreshold)
        {
            Debug.Log("能量不足，無法開啟高能模式");
            return;
        }
        
        // 切換高能模式狀態
        SetHighEnergyMode(!isHighEnergyMode);
    }
    
    void SetHighEnergyMode(bool enable)
    {
        isHighEnergyMode = enable;
        
        if (enable)
        {
            Debug.Log($"高能模式開啟！當前能量: {currentEnergy:F1}/{maxEnergy:F1}");
        }
        else
        {
            Debug.Log($"高能模式關閉。當前能量: {currentEnergy:F1}/{maxEnergy:F1}");
        }
    }
    
    // 補充能量（供咖啡道具使用）
    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        Debug.Log($"補充能量: +{amount:F1}，當前能量: {currentEnergy:F1}/{maxEnergy:F1}");
    }
    
    // 獲取當前能量（供UI使用）
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
    
    // 獲取能量百分比（0-1之間，供UI使用）
    public float GetEnergyPercentage()
    {
        return Mathf.Clamp01(currentEnergy / maxEnergy);
    }
    
    // 是否處於高能模式（供UI使用）
    public bool IsHighEnergyMode()
    {
        return isHighEnergyMode;
    }
    
    // 獲取最大能量（供UI使用）
    public float GetMaxEnergy()
    {
        return maxEnergy;
    }

    //播放角色動畫
    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    
    
    // 在Scene視圖中顯示地面檢測範圍
    void OnDrawGizmosSelected()
    {
        // 繪製主要檢測射線
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = groundCheck.position + Vector3.up * 0.05f;
        Gizmos.DrawRay(rayStart, Vector3.down * 1.1f);
        
        // 繪製額外的檢測射線（針對capsule優化）
        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Vector3[] rayPositions = {
            groundCheck.position + new Vector3(groundCheckWidth * 0.7f, 0.05f, 0),
            groundCheck.position + new Vector3(-groundCheckWidth * 0.7f, 0.05f, 0),
            groundCheck.position + new Vector3(0, 0.05f, groundCheckWidth * 0.7f),
            groundCheck.position + new Vector3(0, 0.05f, -groundCheckWidth * 0.7f)
        };
        
        foreach (Vector3 pos in rayPositions)
        {
            Gizmos.DrawRay(pos, Vector3.down * 1.1f);
        }
        
        // 繪製capsule形狀的檢測範圍
        Gizmos.color = Color.blue;
        Vector3 capsuleCenter = groundCheck.position + Vector3.down * 0.25f;
        Gizmos.DrawWireCube(capsuleCenter, new Vector3(groundCheckWidth * 1.4f, groundCheckHeight, groundCheckWidth * 1.4f));
        
        // 額外繪製capsule的圓柱形邊界
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position + Vector3.down * 0.5f, groundCheckWidth * 0.7f);
    }


    //判斷動畫是否播放完畢(一次)
    public bool IsAnimationCompleted(string _aniName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(_aniName)&&stateInfo.normalizedTime >=1.0);
    }

    //切換當前狀態
    public void SetCurrentState(BaseState state)
    {
        current_state = state;
    }
}
