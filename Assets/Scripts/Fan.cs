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

    // 粒子特效
    public ParticleSystem fanParticle;

    // 可調整吹風方向（度數）
    [Header("Direction Offset (degrees)")]
    [Range(-90f, 90f)] public float directionPitch = 0f; // 上下（正為向上）
    [Range(-180f, 180f)] public float directionYaw = 0f;  // 左右（正為向右）

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
        ParticlePlayIfNeed();
    }

    private void Update()
    {
        if (mode == FanMode.Oscillate)
        {
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
                ParticlePlayIfNeed();
            }
            else if (!isBlowing && timer >= windOffDuration)
            {
                isBlowing = true;
                timer = 0f;
                ParticlePlayIfNeed();
            }
        }
        // 持續風無需處理粒子特效（可自行Always Play）
    }

    private void ParticlePlayIfNeed()
    {
        if (fanParticle == null) return;
        if (mode == FanMode.Intermittent)
        {
            if (isBlowing)
            {
                if (!fanParticle.isPlaying)
                    fanParticle.Play(true); // 立即顯示粒子
            }
            else
            {
                if (fanParticle.isPlaying)
                {
                    fanParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    fanParticle.Clear(true); // 立即清空所有現有粒子
                }
            }
        }
    }

    private Vector3 GetBlowDirection()
    {
        // 先以 transform.forward 為基準，先繞本體 up 軸做 yaw（左右），再繞本體 right 軸做 pitch（上下）
        Vector3 dir = transform.forward;
        dir = Quaternion.AngleAxis(directionYaw, transform.up) * dir;
        dir = Quaternion.AngleAxis(directionPitch, transform.right) * dir;
        return dir.normalized;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        // 間歇模式下沒吹時不理會
        if (mode == FanMode.Intermittent && !isBlowing) return;
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.AddForce(GetBlowDirection() * forceStrength, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 dir = GetBlowDirection();
        float arrowLength = 2f;
        Gizmos.DrawRay(start, dir * arrowLength);
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left  = Quaternion.LookRotation(dir) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
        Gizmos.DrawRay(start + dir * arrowLength, right * 0.5f);
        Gizmos.DrawRay(start + dir * arrowLength, left  * 0.5f);
    }
}

