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
			}
		}
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
}


