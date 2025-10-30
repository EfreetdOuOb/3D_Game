using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    
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
    
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 moveDirection;
    
    [Header("蓄力狀態")]
    private bool isCharging = false;
    private float currentChargeTime = 0f;
    private float currentJumpForce = 0f;
    
    [Header("跳躍緩衝狀態")]
    private float lastGroundedTime = 0f;     // 最後一次在地面的時間
    private bool wasGrounded = false;        // 上一幀是否在地面
    
    [Header("跳躍啟用（支援地板debuff）")]
    public bool canJump = true;
    
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
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
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
        playerPhysicsMaterial.dynamicFriction = 0.8f;  // 增加摩擦力，適合capsule
        playerPhysicsMaterial.staticFriction = 0.9f;   // 增加靜摩擦力
        playerPhysicsMaterial.bounciness = 0.1f;       // 稍微增加彈性，模擬capsule特性
        playerPhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Maximum; // 使用最大摩擦力
        playerPhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            playerCollider.material = playerPhysicsMaterial;
        }
    }
    
    void Update()
    {
        // 檢測是否在地面上
        CheckGrounded();
        
        // 更新土狼時間
        UpdateCoyoteTime();
        
        // 獲取輸入
        HandleInput();
        
        // 移動角色
        MovePlayer();
        
        // 蓄力跳躍
        HandleChargeJump();
    }
    
    void CheckGrounded()
    {
        // 使用射線檢測地面，針對capsule調整檢測距離
        Vector3 rayStart = transform.position + Vector3.up * 0.05f; // capsule需要更小的偏移
        isGrounded = Physics.Raycast(rayStart, Vector3.down, 1.1f, groundMask); // 調整檢測距離
        
        // 額外的射線檢測，針對capsule形狀優化
        Vector3[] rayPositions = {
            transform.position + new Vector3(groundCheckWidth * 0.7f, 0.05f, 0), // 縮小檢測範圍
            transform.position + new Vector3(-groundCheckWidth * 0.7f, 0.05f, 0),
            transform.position + new Vector3(0, 0.05f, groundCheckWidth * 0.7f),
            transform.position + new Vector3(0, 0.05f, -groundCheckWidth * 0.7f)
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
    
    void HandleInput()
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
    
    void MovePlayer()
    {
        // 應用移動速度
        Vector3 move = moveDirection * moveSpeed;
        
        // 只影響X和Z軸，保持重力效果
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        
        // 讓角色面向移動方向
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void HandleChargeJump()
    {
        // 開始蓄力（按下跳躍鍵）
        if (Input.GetButtonDown("Jump"))
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
        if (Input.GetButton("Jump") && isCharging)
        {
            ContinueCharging();
        }
        
        // 釋放跳躍（鬆開跳躍鍵）
        if (Input.GetButtonUp("Jump") && isCharging)
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
            // 重置Y軸速度，確保跳躍高度一致
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
            
            float chargeProgress = GetChargeProgress();
            bool usingCoyoteTime = !isGrounded && (Time.time - lastGroundedTime <= coyoteTime);
            string jumpType = usingCoyoteTime ? " [土狼時間]" : " [地面跳躍]";
            Debug.Log($"跳躍力度: {currentJumpForce:F1} (蓄力時間: {currentChargeTime:F2}s, 進度: {chargeProgress:P0}){jumpType}");
            
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
    
    void UpdateCoyoteTime()
    {
        // 記錄地面狀態變化
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
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
    
    
    
    // 在Scene視圖中顯示地面檢測範圍
    void OnDrawGizmosSelected()
    {
        // 繪製主要檢測射線
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.05f;
        Gizmos.DrawRay(rayStart, Vector3.down * 1.1f);
        
        // 繪製額外的檢測射線（針對capsule優化）
        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Vector3[] rayPositions = {
            transform.position + new Vector3(groundCheckWidth * 0.7f, 0.05f, 0),
            transform.position + new Vector3(-groundCheckWidth * 0.7f, 0.05f, 0),
            transform.position + new Vector3(0, 0.05f, groundCheckWidth * 0.7f),
            transform.position + new Vector3(0, 0.05f, -groundCheckWidth * 0.7f)
        };
        
        foreach (Vector3 pos in rayPositions)
        {
            Gizmos.DrawRay(pos, Vector3.down * 1.1f);
        }
        
        // 繪製capsule形狀的檢測範圍
        Gizmos.color = Color.blue;
        Vector3 capsuleCenter = transform.position + Vector3.down * 0.25f;
        Gizmos.DrawWireCube(capsuleCenter, new Vector3(groundCheckWidth * 1.4f, groundCheckHeight, groundCheckWidth * 1.4f));
        
        // 額外繪製capsule的圓柱形邊界
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.5f, groundCheckWidth * 0.7f);
    }
}
