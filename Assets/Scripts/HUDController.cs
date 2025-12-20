using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
	[Header("目標")]
	public PlayerMove player;						// 目標玩家（可不填，將自動尋找）

	[Header("蓄力跳躍")]
	public Slider chargeSlider;						// 顯示蓄力進度（0~1）
	public bool hideChargeWhenIdle = true;			// 非蓄力時隱藏
	public Image chargeFillImage;					// 可選：填充圖，用於顏色漸變
	public Gradient chargeColorGradient;			// 可選：蓄力顏色
	public bool hideChargeHandle = true;			// 是否隱藏蓄力 Slider 手把

	[Header("能量")]
	public Slider energySlider;						// 顯示能量條（0~maxEnergy）
	public Image energyFillImage;					// 可選：填充圖，用於顏色漸變
	public Gradient energyColorGradient;			// 可選：能量顏色
	public bool hideEnergyHandle = true;			// 是否隱藏能量 Slider 手把
	[Header("能量自動關閉門檻（視覺標記）")]
	public RectTransform energyThresholdMarker;		// 門檻指示線（建議為 Image）
	public float thresholdLineWidth = 2f;			// 指示線寬度（像素）
	public bool showThresholdMarker = true;			// 是否顯示門檻指示線

	[Header("數值修正")]
	public float zeroEpsilon = 0.001f;				// 小於此值視為 0，避免 UI 殘留

	void Awake()
	{
		// 自動尋找 PlayerMove
		if (player == null)
		{
			player = FindFirstObjectByType<PlayerMove>();
		}
	}

	void Start()
	{
		// 初始化能量條上限
		if (player != null && energySlider != null)
		{
			energySlider.minValue = 0f;
			energySlider.maxValue = Mathf.Max(1f, player.GetMaxEnergy());
			energySlider.wholeNumbers = false;

			// 選擇性隱藏手把
			if (hideEnergyHandle && energySlider.handleRect != null)
			{
				energySlider.handleRect.gameObject.SetActive(false);
				FixSliderLayout(energySlider);
			}
		}

		// 初始化蓄力條
		if (chargeSlider != null)
		{
			chargeSlider.minValue = 0f;
			chargeSlider.maxValue = 1f;
			chargeSlider.wholeNumbers = false;
			if (hideChargeWhenIdle) chargeSlider.gameObject.SetActive(false);
			if (hideChargeHandle && chargeSlider.handleRect != null)
			{
				chargeSlider.handleRect.gameObject.SetActive(false);
				FixSliderLayout(chargeSlider);
			}
		}

		// 初始化門檻指示線
		UpdateEnergyThresholdMarker();
	}

	void Update()
	{
		if (player == null) return;

		// 更新能量
		if (energySlider != null)
		{
			float currentEnergy = Mathf.Max(0f, player.GetCurrentEnergy());
			float maxEnergy = Mathf.Max(1f, player.GetMaxEnergy());
			energySlider.maxValue = maxEnergy; // 若設計中上限會變動，持續同步
			
			// 避免 0 附近浮點殘留造成 UI 還有一點點
			if (currentEnergy <= zeroEpsilon)
			{
				energySlider.value = 0f;
				energySlider.normalizedValue = 0f;
			}
			else
			{
				energySlider.value = currentEnergy;
			}

			if (energyFillImage != null && energyColorGradient != null)
			{
				float t = Mathf.Clamp01(currentEnergy / maxEnergy);
				energyFillImage.color = energyColorGradient.Evaluate(t);
			}

			// 門檻指示線位置（若上限或門檻變動，持續同步）
			UpdateEnergyThresholdMarker();
		}

		// 更新蓄力
		if (chargeSlider != null)
		{
			bool isCharging = player.IsCharging();
			if (hideChargeWhenIdle) chargeSlider.gameObject.SetActive(isCharging);

			float progress = player.GetChargeProgress(); // 0~1
			// 避免接近 0 的殘留
			if (progress <= zeroEpsilon) 
			{
				chargeSlider.value = 0f;
				chargeSlider.normalizedValue = 0f;
			}
			else
			{
				chargeSlider.value = progress;
			}

			if (chargeFillImage != null && chargeColorGradient != null)
			{
				chargeFillImage.color = chargeColorGradient.Evaluate(progress);
			}
		}
	}

	void UpdateEnergyThresholdMarker()
	{
		if (!showThresholdMarker || energyThresholdMarker == null || energySlider == null || player == null) return;

		// 取得門檻與上限
		float maxEnergy = Mathf.Max(1f, player.GetMaxEnergy());
		float threshold = Mathf.Clamp(player.minEnergyThreshold, 0f, maxEnergy);
		float t = Mathf.Clamp01(threshold / maxEnergy); // 規範化 0~1

		// 指示線錨點對齊到 t 位置（相對於 Slider 的填充區域）
		// 要求在編輯器中將 energyThresholdMarker 放在能量條對齊的容器內（通常是 Fill Area 或 Slider）
		energyThresholdMarker.anchorMin = new Vector2(t, 0f);
		energyThresholdMarker.anchorMax = new Vector2(t, 1f);
		energyThresholdMarker.pivot = new Vector2(0.5f, 0.5f);
		energyThresholdMarker.anchoredPosition = Vector2.zero;
		energyThresholdMarker.sizeDelta = new Vector2(thresholdLineWidth, 1f); // 給定線寬，垂直方向全滿

		// 若方向反轉（RightToLeft），位置需鏡像
		if (energySlider.direction == Slider.Direction.RightToLeft)
		{
			float mirroredT = 1f - t;
			energyThresholdMarker.anchorMin = new Vector2(mirroredT, 0f);
			energyThresholdMarker.anchorMax = new Vector2(mirroredT, 1f);
		}
	}

	// 當隱藏手把時，移除預設 Fill Area 左/右邊距，避免左端缺一塊
	void FixSliderLayout(Slider slider)
	{
		// 調整 Fill Rect 使其橫向鋪滿
		if (slider.fillRect != null)
		{
			RectTransform fill = slider.fillRect;
			// 讓 Fill 垂直與 Fill Area 完全對齊，避免高度外溢
			fill.anchorMin = new Vector2(0f, 0f);
			fill.anchorMax = new Vector2(1f, 1f);
			fill.pivot = new Vector2(0.5f, 0.5f);
			fill.offsetMin = Vector2.zero;
			fill.offsetMax = Vector2.zero;
			fill.localScale = Vector3.one;
			fill.anchoredPosition = Vector2.zero;
		}

		// 同時把 Fill Area 的左右 padding 清零（Unity 預設 Slider 會有）
		if (slider.fillRect != null && slider.fillRect.parent is RectTransform fillArea)
		{
			// 只調整左右，保留原本高度/垂直錨點設定
			fillArea.anchorMin = new Vector2(0f, fillArea.anchorMin.y);
			fillArea.anchorMax = new Vector2(1f, fillArea.anchorMax.y);
			fillArea.pivot = new Vector2(0.5f, 0.5f);
			fillArea.offsetMin = new Vector2(0f, fillArea.offsetMin.y);
			fillArea.offsetMax = new Vector2(0f, fillArea.offsetMax.y);
		}
	}
}


