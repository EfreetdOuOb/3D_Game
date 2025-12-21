using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameClear : MonoBehaviour
{
    [Header("通關 UI")]
    [Tooltip("通關成功時顯示的 UI 面板")]
    public GameObject gameClearPanel;
    
    [Tooltip("顯示通關時間的文字元件（在通關 UI 面板中）")]
    public TextMeshProUGUI timeDisplayText;
    
    [Header("計時器 UI")]
    [Tooltip("遊戲中顯示計時器的文字元件（可選）")]
    public TextMeshProUGUI timerText;
    
    [Header("音效與特效")]
    [Tooltip("通關音效")]
    public AudioSource clearSound;
    
    [Tooltip("通關特效")]
    public ParticleSystem clearEffect;
    
    [Header("通關後行為")]
    [Tooltip("通關後自動載入下一關的場景名稱（為空則不自動載入）")]
    public string nextSceneName = "";
    
    [Tooltip("自動載入下一關的延遲時間（秒）")]
    public float autoLoadNextSceneDelay = 3f;
    
    [Header("玩家檢測")]
    [Tooltip("玩家層級")]
    public LayerMask playerLayer = 1;
    
    [Tooltip("玩家標籤")]
    public string playerTag = "Player";
    
    [Tooltip("玩家物件（可選，為空則自動尋找）")]
    public PlayerMove playerMove;
    
    // 內部狀態
    private bool isCleared = false;                    // 是否已通關
    private float gameStartTime;                       // 遊戲開始時間
    private float clearTime;                           // 通關時間（秒）
    private Collider triggerCollider;                  // Trigger 碰撞器
    
    void Start()
    {
        // 記錄遊戲開始時間
        gameStartTime = Time.time;
        
        // 自動尋找玩家（如果未指定）
        if (playerMove == null)
        {
            playerMove = FindFirstObjectByType<PlayerMove>();
        }
        
        // 確保有 Collider 組件且設為 Trigger
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogWarning("GameClear: 未找到 Collider 組件！將自動添加 BoxCollider。");
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        triggerCollider.isTrigger = true;
        
        // 初始化通關 UI（如果有的話）
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(false);
        }
        
        // 初始化計時器文字（如果有的話）
        if (timerText != null)
        {
            timerText.text = "00:00.00";
        }
    }
    
    void Update()
    {
        // 如果已通關，停止計時器更新
        if (isCleared) return;
        
        // 更新計時器顯示
        UpdateTimerDisplay();
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            float elapsedTime = Time.time - gameStartTime;
            timerText.text = FormatTime(elapsedTime);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 如果已通關，不再處理
        if (isCleared) return;
        
        // 檢查是否為玩家
        if (!IsPlayer(other)) return;
        
        // 觸發通關
        TriggerGameClear();
    }
    
    bool IsPlayer(Collider other)
    {
        // 檢查標籤
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            return true;
        }
        
        // 檢查層級
        if (playerLayer != 0 && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            return true;
        }
        
        return false;
    }
    
    void TriggerGameClear()
    {
        if (isCleared) return;
        
        isCleared = true;
        
        // 記錄通關時間
        clearTime = Time.time - gameStartTime;
        
        Debug.Log($"恭喜！遊戲通關！通關時間: {FormatTime(clearTime)}");
        
        // 禁用玩家輸入並停止移動（不暫停遊戲，這樣音效可以正常播放）
        if (playerMove != null)
        {
            playerMove.SetInputEnabled(false);
            playerMove.StopPlayerMovement();
        }
        
        // 播放音效（確保音效可以正常播放，因為沒有暫停遊戲）
        if (clearSound != null)
        {
            clearSound.Play();
        }
        
        // 播放特效
        if (clearEffect != null)
        {
            clearEffect.Play();
        }
        
        // 顯示通關 UI
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
        
        // 更新通關時間顯示
        if (timeDisplayText != null)
        {
            timeDisplayText.text = FormatTime(clearTime);
        }
        
        // 確保游標可見且可點擊（用於點擊 UI 按鈕）
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // 自動載入下一關（如果設定了）
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Invoke("LoadNextScene", autoLoadNextSceneDelay);
        }
    }
    
    void LoadNextScene()
    {
        // 恢復玩家輸入（如果有的話）
        if (playerMove != null)
        {
            playerMove.SetInputEnabled(true);
        }
        
        // 載入下一關
        SceneManager.LoadScene(nextSceneName);
    }
    
    // 格式化時間為 MM:SS.mm 格式
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds % 1f) * 100f);
        
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
    
    // 公開方法：手動觸發通關（可由其他腳本調用）
    public void ForceGameClear()
    {
        TriggerGameClear();
    }
    
    // 公開方法：檢查是否已通關
    public bool IsCleared()
    {
        return isCleared;
    }
    
    // 公開方法：獲取當前遊戲時間（秒）
    public float GetCurrentTime()
    {
        if (isCleared)
        {
            return clearTime;
        }
        return Time.time - gameStartTime;
    }
    
    // 公開方法：獲取通關時間（秒），如果未通關則返回 -1
    public float GetClearTime()
    {
        return isCleared ? clearTime : -1f;
    }
    
    // 公開方法：重置計時器（用於重新開始遊戲）
    public void ResetTimer()
    {
        gameStartTime = Time.time;
        isCleared = false;
        clearTime = 0f;
    }
}
