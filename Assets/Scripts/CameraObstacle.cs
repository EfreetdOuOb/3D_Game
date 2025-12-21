using UnityEngine;

/// <summary>
/// 攝像機障礙物標記腳本
/// 掛在牆壁或其他障礙物上，用於標記這些物件會阻擋攝像機
/// 配合 Cinemachine Collider 使用
/// </summary>
[RequireComponent(typeof(Collider))]
public class CameraObstacle : MonoBehaviour
{
    [Header("障礙物設定")]
    [Tooltip("是否啟用此障礙物（可以臨時禁用）")]
    public bool isActive = true;
    
    [Tooltip("障礙物優先級（數值越大，優先級越高）")]
    public int priority = 0;
    
    [Header("視覺標記")]
    [Tooltip("在 Scene 視圖中顯示障礙物範圍")]
    public bool showGizmos = true;
    
    [Tooltip("Gizmo 顏色")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    
    private Collider obstacleCollider;
    
    void Awake()
    {
        obstacleCollider = GetComponent<Collider>();
        
        // 確保 Collider 不是 Trigger（障礙物應該是實體碰撞）
        if (obstacleCollider != null && obstacleCollider.isTrigger)
        {
            Debug.LogWarning($"CameraObstacle: {gameObject.name} 的 Collider 設為 Trigger，建議改為實體碰撞以正確阻擋攝像機。");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos || !isActive) return;
        
        // 獲取 Collider 的邊界
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        Gizmos.color = gizmoColor;
        
        // 根據 Collider 類型繪製不同的 Gizmo
        if (col is BoxCollider)
        {
            BoxCollider boxCol = col as BoxCollider;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphereCol = col as SphereCollider;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
        }
        else if (col is CapsuleCollider)
        {
            CapsuleCollider capsuleCol = col as CapsuleCollider;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            // 簡化顯示為包圍盒
            Vector3 size = new Vector3(capsuleCol.radius * 2, capsuleCol.height, capsuleCol.radius * 2);
            Gizmos.DrawCube(capsuleCol.center, size);
        }
        else if (col is MeshCollider)
        {
            MeshCollider meshCol = col as MeshCollider;
            if (meshCol.sharedMesh != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireMesh(meshCol.sharedMesh);
            }
        }
        
        // 重置矩陣
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!isActive) return;
        
        // 選中時顯示更明顯的標記
        Gizmos.color = Color.red;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
    
    // 公開方法：啟用/禁用障礙物
    public void SetActive(bool active)
    {
        isActive = active;
        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = active;
        }
    }
    
    // 公開方法：檢查是否啟用
    public bool IsActive()
    {
        return isActive && obstacleCollider != null && obstacleCollider.enabled;
    }
}
