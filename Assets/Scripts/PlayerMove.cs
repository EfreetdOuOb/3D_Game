using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rotationSpeed = 100f;
    
    [Header("地面檢測")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 moveDirection;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 如果沒有地面檢測點，創建一個
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // 設定物理材質，避免在斜坡上滑動
        PhysicsMaterial playerPhysicsMaterial = new PhysicsMaterial("PlayerPhysics");
        playerPhysicsMaterial.dynamicFriction = 0.6f;
        playerPhysicsMaterial.staticFriction = 0.6f;
        playerPhysicsMaterial.bounciness = 0f;
        playerPhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Average;
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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }
    
    void HandleInput()
    {
        // 獲取WASD輸入
        float horizontal = Input.GetAxis("Horizontal"); // A/D 或 左/右箭頭
        float vertical = Input.GetAxis("Vertical");     // W/S 或 上/下箭頭
        
        // 計算移動方向（相對於攝像機）
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
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
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
