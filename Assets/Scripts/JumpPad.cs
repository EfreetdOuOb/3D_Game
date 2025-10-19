using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("跳墊設定")]
    public float jumpForce = 20f;           // 跳墊力度
    public float upwardForce = 15f;          // 向上力度
    public float forwardForce = 5f;          // 向前力度
    public bool usePlayerDirection = true;    // 是否使用玩家移動方向
    public float cooldownTime = 0.5f;        // 冷卻時間
    
    [Header("視覺效果")]
    public ParticleSystem jumpEffect;        // 跳躍特效
    public AudioSource jumpSound;            // 跳躍音效
    public float animationScale = 1.2f;       // 動畫縮放
    public float animationDuration = 0.3f;   // 動畫持續時間
    
    [Header("檢測設定")]
    public float detectionRadius = 1f;           // 檢測半徑
    public LayerMask playerLayer = 1;        // 玩家層級
    
    private bool isOnCooldown = false;
    private float lastJumpTime = 0f;
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = transform.localScale;
        
        // 如果沒有音效組件，創建一個
        if (jumpSound == null)
        {
            jumpSound = gameObject.AddComponent<AudioSource>();
            jumpSound.playOnAwake = false;
        }
        
        // 如果沒有特效，嘗試找到子物件中的特效
        if (jumpEffect == null)
        {
            jumpEffect = GetComponentInChildren<ParticleSystem>();
        }
    }
    
    void Update()
    {
        // 檢查冷卻時間
        if (isOnCooldown && Time.time - lastJumpTime >= cooldownTime)
        {
            isOnCooldown = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 檢查是否為玩家且不在冷卻中
        if (IsPlayer(other) && !isOnCooldown)
        {
            // 檢查玩家是否在跳墊正上方
            if (IsPlayerAboveJumpPad(other))
            {
                ApplyJumpForce(other);
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // 持續檢測，確保玩家在正上方時能觸發
        if (IsPlayer(other) && !isOnCooldown)
        {
            if (IsPlayerAboveJumpPad(other))
            {
                ApplyJumpForce(other);
            }
        }
    }
    
    bool IsPlayer(Collider other)
    {
        // 檢查是否為玩家層級
        return ((1 << other.gameObject.layer) & playerLayer) != 0;
    }
    
    bool IsPlayerAboveJumpPad(Collider player)
    {
        // 計算玩家相對於跳墊的位置
        Vector3 playerPos = player.transform.position;
        Vector3 jumpPadPos = transform.position;
        
        // 計算水平距離
        float horizontalDistance = Vector2.Distance(
            new Vector2(playerPos.x, playerPos.z),
            new Vector2(jumpPadPos.x, jumpPadPos.z)
        );
        
        // 檢查是否在檢測半徑內
        return horizontalDistance <= detectionRadius;
    }
    
    void ApplyJumpForce(Collider player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null) return;
        
        // 計算跳躍方向
        Vector3 jumpDirection = Vector3.up;
        
        if (usePlayerDirection)
        {
            // 獲取玩家移動方向
            PlayerMove playerMove = player.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // 獲取玩家移動方向，但主要向上
                Vector3 playerDirection = playerMove.transform.forward;
                jumpDirection = (Vector3.up + playerDirection * 0.3f).normalized;
            }
        }
        
        // 應用跳躍力度
        Vector3 jumpForceVector = jumpDirection * jumpForce;
        playerRb.AddForce(jumpForceVector, ForceMode.Impulse);
        
        // 應用額外的向上力度
        playerRb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        
        // 如果玩家有移動，添加向前力度
        if (usePlayerDirection)
        {
            Vector3 forwardDirection = player.transform.forward;
            playerRb.AddForce(forwardDirection * forwardForce, ForceMode.Impulse);
        }
        
        // 觸發視覺和音效效果
        TriggerEffects();
        
        // 設置冷卻
        isOnCooldown = true;
        lastJumpTime = Time.time;
        
        Debug.Log($"跳墊觸發！力度: {jumpForce}, 玩家: {player.name}");
    }
    
    void TriggerEffects()
    {
        // 播放音效
        if (jumpSound != null)
        {
            jumpSound.Play();
        }
        
        // 播放特效
        if (jumpEffect != null)
        {
            jumpEffect.Play();
        }
        
        // 播放動畫
        StartCoroutine(PlayJumpAnimation());
    }
    
    System.Collections.IEnumerator PlayJumpAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Vector3 targetScale = originalScale * animationScale;
        
        // 放大
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration / 2);
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // 縮小回原尺寸
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration / 2);
            transform.localScale = Vector3.Lerp(targetScale, startScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    // 在Scene視圖中顯示檢測範圍
    void OnDrawGizmosSelected()
    {
        // 繪製檢測範圍
        Gizmos.color = isOnCooldown ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // 繪製跳躍方向
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);
        
        // 繪製跳墊邊界
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
