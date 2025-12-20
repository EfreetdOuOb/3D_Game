using UnityEngine;

public class MouseParallax : MonoBehaviour
{
    public float floatAmplitude = 20f;   // 上下飄的高度（AnchoredPosition 單位）
    public float floatFrequency = 0.5f;  // 上下飄的速度
    public float rotateAmplitude = 2f;   // 左右小角度旋轉
    public float rotateFrequency = 0.3f; // 旋轉速度

    RectTransform rect;
    Vector2 startPos;
    float startTime;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        startTime = Random.value * 10f; // 每個物件起始位相不同，感覺更自然
    }

    void Update()
    {
        float t = Time.time + startTime;

        // 上下飄
        float offsetY = Mathf.Sin(t * Mathf.PI * floatFrequency) * floatAmplitude;

        // 輕微旋轉（Z 軸）
        float angle = Mathf.Sin(t * Mathf.PI * rotateFrequency) * rotateAmplitude;

        rect.anchoredPosition = startPos + new Vector2(0f, offsetY);
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}

