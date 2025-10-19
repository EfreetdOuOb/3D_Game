using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("跳墊設定")]
    public float jumpForce = 20f;            // 跳墊力度
    public float bounceMultiplier = 1.5f;    // 彈跳倍數
    public bool preserveHorizontalVelocity = true;  // 保持水平速度
    public bool addUpwardForce = true;       // 添加向上力度
    public float cooldownTime = 0.3f;        // 冷卻時間
    
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
        
        // 獲取當前速度
        Vector3 currentVelocity = playerRb.linearVelocity;
        
        // 計算新的速度
        Vector3 newVelocity = currentVelocity;
        
        // 如果保持水平速度
        if (preserveHorizontalVelocity)
        {
            // 保持X和Z軸速度，只改變Y軸
            newVelocity.y = jumpForce;
        }
        else
        {
            // 完全重置速度，只向上
            newVelocity = Vector3.up * jumpForce;
        }
        
        // 如果添加向上力度
        if (addUpwardForce)
        {
            newVelocity.y += jumpForce * bounceMultiplier;
        }
        
        // 應用新速度
        playerRb.linearVelocity = newVelocity;
        
        // 觸發視覺和音效效果
        TriggerEffects();
        
        // 設置冷卻
        isOnCooldown = true;
        lastJumpTime = Time.time;
        
        Debug.Log($"跳墊觸發！力度: {jumpForce}, 新Y速度: {newVelocity.y:F2}, 玩家: {player.name}");
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
        
        // 繪製跳躍力度
        Gizmos.color = Color.yellow;
        float jumpHeight = jumpForce / 5f; // 縮放顯示
        Gizmos.DrawRay(transform.position, Vector3.up * jumpHeight);
        
        // 繪製跳墊邊界
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        // 繪製彈跳倍數效果
        if (addUpwardForce)
        {
            Gizmos.color = Color.cyan;
            float bounceHeight = (jumpForce * bounceMultiplier) / 5f;
            Gizmos.DrawRay(transform.position + Vector3.right * 0.3f, Vector3.up * bounceHeight);
        }
    }
}
