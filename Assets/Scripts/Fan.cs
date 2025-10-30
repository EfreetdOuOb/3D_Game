using UnityEngine;

public class Fan : MonoBehaviour
{
    public enum FanMode
    {
        Continuous,
        Oscillate,
        Intermittent
    }
    public FanMode mode = FanMode.Continuous;
    public float forceStrength = 10f;

    // Oscillate 模式參數
    public float oscillateAngle = 45f; // 左右最大搖擺角度
    [Min(0.01f)] public float oscillateSpeed = 1.0f; // 不可為0
    private float baseY;

    // Intermittent 模式參數
    public float windOnDuration = 1.0f;
    public float windOffDuration = 1.0f;
    private float timer = 0f;
    private bool isBlowing = true;

    private void Start()
    {
        baseY = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (mode == FanMode.Oscillate)
        {
            // 防呆: 避免oscillateSpeed為0產生錯誤
            if (oscillateSpeed <= 0.0f)
                return;
            float oscPhase = Mathf.Sin(Time.time * Mathf.PI * 2 / oscillateSpeed);
            float yRot = baseY + oscPhase * oscillateAngle;
            Vector3 euler = transform.eulerAngles;
            euler.y = yRot;
            transform.eulerAngles = euler;
        }
        else if (mode == FanMode.Intermittent)
        {
            timer += Time.deltaTime;
            if (isBlowing && timer >= windOnDuration)
            {
                isBlowing = false;
                timer = 0f;
            }
            else if (!isBlowing && timer >= windOffDuration)
            {
                isBlowing = true;
                timer = 0f;
            }
        }
        // Continuous 無需特別處理
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        // 間歇模式下沒吹時不理會
        if (mode == FanMode.Intermittent && !isBlowing) return;
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.AddForce(transform.forward.normalized * forceStrength, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 dir = transform.forward;
        float arrowLength = 2f;
        Gizmos.DrawRay(start, dir * arrowLength);
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left  = Quaternion.LookRotation(dir) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
        Gizmos.DrawRay(start + dir * arrowLength, right * 0.5f);
        Gizmos.DrawRay(start + dir * arrowLength, left  * 0.5f);
    }
}
