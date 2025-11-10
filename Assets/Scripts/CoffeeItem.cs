using UnityEngine;
using DG.Tweening;

public class CoffeeItem : MonoBehaviour
{
    [Header("咖啡道具設定")]
    public float energyAmount = 30f;           // 補充的能量數量
    public float respawnTime = 10f;            // 重生時間（秒），0表示不重生
    public bool destroyOnPickup = false;       // 拾取後是否銷毀（不重生）
    
    [Header("視覺效果")]
    public ParticleSystem pickupEffect;        // 拾取特效
    public AudioSource pickupSound;            // 拾取音效
    public float rotationDuration = 2f;        // 旋轉一圈的時間（秒）
    public float floatDuration = 2f;           // 上下漂浮一個週期的時間（秒）
    public float floatAmount = 0.3f;           // 漂浮幅度
    
    [Header("檢測設定")]
    public float detectionRadius = 1.5f;       // 檢測半徑
    public LayerMask playerLayer = 1;          // 玩家層級
    
    private bool isPickedUp = false;           // 是否已被拾取
    private Vector3 startPosition;             // 初始位置
    private Renderer itemRenderer;             // 渲染器組件
    private Collider itemCollider;             // 碰撞器組件
    private Tweener rotationTween;             // 旋轉動畫
    private Tweener floatTween;                // 漂浮動畫
    
    void Start()
    {
        startPosition = transform.position;
        
        // 獲取渲染器和碰撞器組件
        itemRenderer = GetComponent<Renderer>();
        itemCollider = GetComponent<Collider>();
        
        // 如果沒有音效組件，創建一個
        if (pickupSound == null)
        {
            pickupSound = gameObject.AddComponent<AudioSource>();
            pickupSound.playOnAwake = false;
        }
        
        // 如果沒有特效，嘗試找到子物件中的特效
        if (pickupEffect == null)
        {
            pickupEffect = GetComponentInChildren<ParticleSystem>();
        }
        
        // 確保有碰撞器
        if (itemCollider == null)
        {
            // 創建一個球型碰撞器
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = detectionRadius;
            itemCollider = sphereCollider;
        }
        else
        {
            // 確保碰撞器是觸發器
            itemCollider.isTrigger = true;
        }
        
        // 啟動 DOTween 動畫
        StartAnimations();
    }
    
    void StartAnimations()
    {
        // 旋轉動畫：繞 Y 軸無限旋轉
        // 使用相對旋轉，每次循環旋轉 360 度，然後重新開始
        rotationTween = transform.DORotate(new Vector3(0f, 360f, 0f), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative(true);
        
        // 漂浮動畫：上下浮動（從起始位置開始）
        transform.position = startPosition;
        floatTween = transform.DOMoveY(startPosition.y + floatAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);
    }
    
    void StopAnimations()
    {
        // 停止旋轉動畫
        if (rotationTween != null && rotationTween.IsActive())
        {
            rotationTween.Kill();
        }
        
        // 停止漂浮動畫
        if (floatTween != null && floatTween.IsActive())
        {
            floatTween.Kill();
        }
    }
    
    void OnDestroy()
    {
        // 清理動畫，避免內存洩漏
        StopAnimations();
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 檢查是否為玩家且未被拾取
        if (!isPickedUp && IsPlayer(other))
        {
            PickupCoffee(other);
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // 持續檢測，確保玩家觸碰時能拾取
        if (!isPickedUp && IsPlayer(other))
        {
            PickupCoffee(other);
        }
    }
    
    bool IsPlayer(Collider other)
    {
        // 檢查是否為玩家層級
        return ((1 << other.gameObject.layer) & playerLayer) != 0;
    }
    
    void PickupCoffee(Collider player)
    {
        // 獲取玩家的 PlayerMove 組件
        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove == null)
        {
            // 嘗試從父物件獲取
            playerMove = player.GetComponentInParent<PlayerMove>();
        }
        
        if (playerMove != null)
        {
            // 補充能量
            playerMove.AddEnergy(energyAmount);
            
            // 標記為已拾取
            isPickedUp = true;
            
            // 停止動畫
            StopAnimations();
            
            // 觸發視覺和音效效果
            TriggerPickupEffects();
            
            // 隱藏物件
            SetItemVisibility(false);
            
            // 如果設置為拾取後銷毀，則銷毀物件
            if (destroyOnPickup)
            {
                Destroy(gameObject, 2f); // 延遲銷毀，讓特效播放完
            }
            else if (respawnTime > 0f)
            {
                // 設置重生
                Invoke("RespawnCoffee", respawnTime);
            }
            
            Debug.Log($"玩家拾取咖啡！補充能量: +{energyAmount:F1}");
        }
        else
        {
            Debug.LogWarning("無法找到玩家的 PlayerMove 組件！");
        }
    }
    
    void RespawnCoffee()
    {
        // 重置狀態
        isPickedUp = false;
        
        // 重置位置到起始位置（確保位置正確）
        transform.position = startPosition;
        transform.rotation = Quaternion.identity; // 重置旋轉，讓動畫從初始狀態開始
        
        // 顯示物件
        SetItemVisibility(true);
        
        // 重新啟動動畫
        StartAnimations();
        
        Debug.Log("咖啡道具重生！");
    }
    
    void SetItemVisibility(bool visible)
    {
        // 設置渲染器可見性
        if (itemRenderer != null)
        {
            itemRenderer.enabled = visible;
        }
        
        // 設置碰撞器啟用狀態
        if (itemCollider != null)
        {
            itemCollider.enabled = visible;
        }
        
        // 設置所有子物件的可見性（除了特效）
        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null && child.GetComponent<ParticleSystem>() == null)
            {
                childRenderer.enabled = visible;
            }
        }
    }
    
    void TriggerPickupEffects()
    {
        // 播放音效
        if (pickupSound != null)
        {
            pickupSound.Play();
        }
        
        // 播放特效
        if (pickupEffect != null)
        {
            pickupEffect.Play();
        }
    }
    
    // 在Scene視圖中顯示檢測範圍
    void OnDrawGizmosSelected()
    {
        // 繪製檢測範圍
        Gizmos.color = isPickedUp ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // 繪製能量補充量指示線
        Gizmos.color = Color.yellow;
        Vector3 labelPos = transform.position + Vector3.up * (detectionRadius + 0.5f);
        Gizmos.DrawLine(transform.position, labelPos);
        Gizmos.DrawWireSphere(labelPos, 0.1f);
    }
}

