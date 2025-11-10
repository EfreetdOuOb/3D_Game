using UnityEngine;

public class UIManager : MonoBehaviour
{
	[Header("標題面板與設定面板")]
	public GameObject titlePanel;			// 標題主選單（開始/設定/離開）
	public GameObject settingsPanel;		// 設定面板（從標題開啟或關閉）

	[Header("暫停面板")]
	public GameObject pausePanel;

	void Start()
	{
		// 初始化顯示狀態
		SetPausePanel(false);
		SetSettingsPanel(false); // 預設關閉設定面板（若在標題要預設開啟，可在 Inspector 勾選）

		// 訂閱 GameManager 的暫停事件
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnPauseChanged += HandlePauseChanged;
			GameManager.Instance.OnSettingsToggled += HandleSettingsToggled;
			// 若進入場景時已經是暫停狀態，立即同步
			if (GameManager.Instance.IsPaused) SetPausePanel(true);
		}
	}

	void OnDestroy()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnPauseChanged -= HandlePauseChanged;
			GameManager.Instance.OnSettingsToggled -= HandleSettingsToggled;
		}
	}

	void HandlePauseChanged(bool isPaused)
	{
		SetPausePanel(isPaused);
	}

	void HandleSettingsToggled(bool open)
	{
		SetSettingsPanel(open);

		// 開啟設定時可選擇隱藏標題主選單（視 UI 流程而定）
		if (titlePanel != null) titlePanel.SetActive(!open);
	}

	void SetPausePanel(bool show)
	{
		if (pausePanel != null)
		{
			pausePanel.SetActive(show);
		}
	}

	void SetSettingsPanel(bool show)
	{
		if (settingsPanel != null)
		{
			settingsPanel.SetActive(show);
		}
	}

	// 給 UI Button 綁定：繼續遊戲
	public void OnResumeButton()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.ResumeGame();
		}
	}

	// 給 UI Button 綁定：返回標題
	public void OnReturnToTitleButton()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.ReturnToTitle();
		}
	}

	// ———————————— 標題面板 UI 綁定 ————————————

	// 開始遊戲
	public void OnStartGameButton()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.StartGame();
		}

		// 進入遊戲時可隱藏標題 UI
		if (titlePanel != null) titlePanel.SetActive(false);
		if (settingsPanel != null) settingsPanel.SetActive(false);
	}

	// 開啟設定
	public void OnOpenSettingsButton()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OpenSettings();
		}
	}

	// 關閉設定（返回標題主選單）
	public void OnCloseSettingsButton()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.CloseSettings();
		}

		// 關閉設定後返回標題主 UI
		if (titlePanel != null) titlePanel.SetActive(true);
	}

	// 離開遊戲
	public void OnQuitButton()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.QuitGame();
		}
	}
}


