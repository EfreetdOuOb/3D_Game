using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingHook : MonoBehaviour
{
    [Header("鉤索設定")]
    [Tooltip("鉤索最大射程")]
    public float maxGrappleDistance = 20f;
    [Tooltip("鉤索飛行速度（視覺效果）")]
    public float hookSpeed = 40f;
    [Tooltip("拉向目標點的速度")]
    public float pullSpeed = 15f;
    [Tooltip("到達目標點的判定距離")]
    public float reachThreshold = 1.5f;
    [Tooltip("鉤索可抓取的層級")]
    public LayerMask grappleLayer;
    
    [Header("引用")]
    [Tooltip("鉤索發射起點（例如角色的手或槍口），為空則預設為自身位置")]
    public Transform gunTip;
    [Tooltip("主攝影機")]
    public Camera playerCamera;

    private LineRenderer lineRenderer;
    private Vector3? grapplePoint = null;
    private Vector3 currentGrapplePosition; // 用於繪製繩索發射動畫

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        if (playerCamera == null) playerCamera = Camera.main;
        if (gunTip == null) gunTip = transform;
    }

    // 嘗試發射鉤索：返回是否成功命中
    public bool TryFireGrapple(out Vector3 hitPoint)
    {
        RaycastHit hit;
        // 從畫面中心發射射線
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleLayer))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = Vector3.zero;
        return false;
    }

    public void StartGrapple(Vector3 point)
    {
        grapplePoint = point;
        currentGrapplePosition = gunTip.position; // 從槍口開始
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
    }

    public void StopGrapple()
    {
        grapplePoint = null;
        lineRenderer.enabled = false;
    }

    // 在 State 的 Update 中調用，處理繩索視覺動畫
    public void UpdateGrappleVisual()
    {
        if (grapplePoint == null) return;

        // 讓繩索頭部飛向目標點（視覺效果）
        currentGrapplePosition = Vector3.MoveTowards(currentGrapplePosition, grapplePoint.Value, hookSpeed * Time.deltaTime);

        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }

    // 在 State 的 FixedUpdate 中調用，處理拉力
    public void ExecutePull(Rigidbody rb)
    {
        if (grapplePoint == null) return;

        Vector3 direction = (grapplePoint.Value - rb.transform.position).normalized;
        
        // 這裡直接修改速度，模擬 Only Up 的強力牽引感
        // 保留一點點原有的 Y 軸速度可以製造擺盪感，但在 Only Up 中通常是直線拉過去
        rb.linearVelocity = direction * pullSpeed;
    }

    public bool HasReachedTarget(Vector3 playerPos)
    {
        if (grapplePoint == null) return false;
        return Vector3.Distance(playerPos, grapplePoint.Value) < reachThreshold;
    }

    // 可視化設計：在 Scene 視窗繪製射程範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // 繪製最大射程球體
        Gizmos.DrawWireSphere(transform.position, maxGrappleDistance);
        
        // 如果有攝像機，繪製瞄準線示意
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Gizmos.DrawRay(ray.origin, ray.direction * maxGrappleDistance);
        }
    }
}
