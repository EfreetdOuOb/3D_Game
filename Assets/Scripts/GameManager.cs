using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("標題場景設定")]
	public string titleSceneName = "MenuScene"; // 返回標題時載入的場景名稱

	[Header("遊戲場景設定")]
	public string gameplaySceneName = "SampleScene"; // 開始遊戲時載入的場景

	[Header("狀態")]
	public bool IsPaused { get; private set; } = false;
	public bool IsInSettings { get; private set; } = false;

	public event Action<bool> OnPauseChanged; // 參數：是否暫停
	public event Action<bool> OnSettingsToggled; // 參數：是否開啟設定

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// 進入標題場景時，確保時間正常流逝與游標顯示
		Time.timeScale = 1f;
		AudioListener.pause = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	void Update()
	{
		// 按下 ESC 切換暫停
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// 若在設定面板中，優先返回上一頁（關閉設定）
			if (IsInSettings)
			{
				CloseSettings();
				return;
			}

			// 標題場景不觸發暫停
			if (SceneManager.GetActiveScene().name == titleSceneName) return;

			TogglePause();
		}
	}

	public void TogglePause()
	{
		if (IsPaused) ResumeGame();
		else PauseGame();
	}

	public void PauseGame()
	{
		if (IsPaused) return;
		IsPaused = true;
		Time.timeScale = 0f;
		AudioListener.pause = true;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		OnPauseChanged?.Invoke(true);
	}

	public void ResumeGame()
	{
		if (!IsPaused) return;
		IsPaused = false;
		Time.timeScale = 1f;
		AudioListener.pause = false;
		// 若遊戲需鎖定滑鼠，請在此調整（或由玩家控制器接手）
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		OnPauseChanged?.Invoke(false);
	}

	public void ReturnToTitle()
	{
		// 保證恢復時間流逝
		Time.timeScale = 1f;
		AudioListener.pause = false;
		IsPaused = false;
		OnPauseChanged?.Invoke(false);
		SceneManager.LoadScene(titleSceneName);

		// 標題場景下顯示游標
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	// ———————————— 標題選單公開 API（可直接綁定 UI 按鈕） ————————————

	// 開始遊戲：載入遊戲場景
	public void StartGame()
	{
		// 確保時間恢復
		Time.timeScale = 1f;
		AudioListener.pause = false;
		IsPaused = false;
		OnPauseChanged?.Invoke(false);

		SceneManager.LoadScene(gameplaySceneName);

		// 進入遊戲時隱藏游標（若你的遊戲需要顯示游標，可移除此段）
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	// 開啟設定：只切換狀態並廣播，由 UI 來顯示設定面板
	public void OpenSettings()
	{
		if (IsInSettings) return;
		IsInSettings = true;
		OnSettingsToggled?.Invoke(true);
	}

	// 關閉設定：只切換狀態並廣播，由 UI 來隱藏設定面板
	public void CloseSettings()
	{
		if (!IsInSettings) return;
		IsInSettings = false;
		OnSettingsToggled?.Invoke(false);
	}

	// 離開遊戲（可直接綁定到按鈕）
	public void QuitGame()
	{
		// 在 Editor 中不會真正退出，打印一條訊息以便測試
#if UNITY_EDITOR
		Debug.Log("QuitGame 呼叫（Editor 模式下不會退出）。");
#else
		Application.Quit();
#endif
	}
}


