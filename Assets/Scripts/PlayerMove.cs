using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rotationSpeed = 100f;
    
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
        
        // 獲取輸入
        HandleInput();
        
        // 移動角色
        MovePlayer();
        
        // 跳躍
        HandleJump();
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
    
    void HandleJump()
    {
        // 空格鍵跳躍，且必須在地面上
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // 重置Y軸速度，確保跳躍高度一致
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
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
