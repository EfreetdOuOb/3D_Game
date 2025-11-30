using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    public enum MoveMode { Triggered = 1, Loop = 2 }

    [Header("Targets")]
    public Transform[] targetPoints;           // 多個目標點（依序訪問）
    public Vector3 targetOffset = new Vector3(0, 0, 5); // 若無 targetPoints，用 offset 作為單一目標

    [Header("Motion")]
    public MoveMode mode = MoveMode.Triggered;
    public float speed = 2f;
    public float arriveThreshold = 0.01f;

    [Header("Triggered Mode (Mode 1)")]
    public float waitAtDestination = 2f;       // 到達後停留時間
    public float disappearDuration = 1f;       // 消失的時間（期間 collider/renderer 關閉）

    [Header("Loop Mode (Mode 2)")]
    public float waitAtEnds = 0f;              // 在往返的兩端停留時間

    // 內部狀態
    Vector3 startPos;
    Vector3 destPos;
    Coroutine runningCoroutine;
    bool isMoving = false;
    int currentPointIndex = 0;

    // 用來記住踩到方塊的物件的原始 parent（支援多個）
    Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();

    void Start()
    {
        startPos = transform.position;
        
        // 決定第一個目標
        if (targetPoints != null && targetPoints.Length > 0)
            destPos = targetPoints[0].position;
        else
            destPos = startPos + targetOffset;

        if (mode == MoveMode.Loop)
        {
            runningCoroutine = StartCoroutine(MoveLoop());
        }
    }

    void Update()
    {
        // 無需在 Update 做實作 — movement 由 coroutine 處理
    }

    void OnCollisionEnter(Collision collision)
    {
        var t = collision.transform;
        if (collision.gameObject.CompareTag("Player"))
        {
            // 記住玩家
            if (!originalParents.ContainsKey(t))
                originalParents[t] = t.parent;

            // 若是 Triggered 模式且還沒在移動，啟動移動
            if (mode == MoveMode.Triggered && !isMoving)
            {
                if (runningCoroutine != null) StopCoroutine(runningCoroutine);
                runningCoroutine = StartCoroutine(MoveOnceAndReset());
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var t = collision.transform;
        if (collision.gameObject.CompareTag("Player"))
        {
            // 持續推動玩家，讓玩家跟著方塊移動
            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb != null && isMoving)
            {
                // 計算方塊當前的移動方向和速度
                Vector3 moveDirection = (isMoving ? (destPos - startPos).normalized : Vector3.zero);
                
                // 只在水平方向推動玩家
                Vector3 platformVelocity = moveDirection * speed;
                
                // 保留玩家的垂直速度，只改變水平速度
                Vector3 playerVelocity = rb.linearVelocity;
                playerVelocity.x = platformVelocity.x;
                playerVelocity.z = platformVelocity.z;
                rb.linearVelocity = playerVelocity;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        var t = collision.transform;
        if (collision.gameObject.CompareTag("Player"))
        {
            // 玩家離開方塊
            if (originalParents.ContainsKey(t))
            {
                originalParents.Remove(t);
            }
        }
    }

    IEnumerator MoveOnceAndReset()
    {
        isMoving = true;
        currentPointIndex = 0;

        // 如果沒有設定 targetPoints，用 targetOffset 作為單一目標
        if (targetPoints == null || targetPoints.Length == 0)
        {
            destPos = startPos + targetOffset;
            yield return StartCoroutine(MoveToDestinationAndWait());
            
            // 消失並回歸
            SetVisualsAndColliders(false);
            if (disappearDuration > 0f) yield return new WaitForSeconds(disappearDuration);
            transform.position = startPos;
            SetVisualsAndColliders(true);
            
            isMoving = false;
            runningCoroutine = null;
            yield break;
        }

        // 依序訪問所有 targetPoints
        while (currentPointIndex < targetPoints.Length)
        {
            destPos = targetPoints[currentPointIndex].position;
            yield return StartCoroutine(MoveToDestinationAndWait());
            currentPointIndex++;
        }

        // 全部走完後消失並回歸起點
        SetVisualsAndColliders(false);
        if (disappearDuration > 0f) yield return new WaitForSeconds(disappearDuration);
        
        transform.position = startPos;
        currentPointIndex = 0;
        
        SetVisualsAndColliders(true);
        isMoving = false;
        runningCoroutine = null;
    }

    IEnumerator MoveToDestinationAndWait()
    {
        // 移動到目標
        while (Vector3.Distance(transform.position, destPos) > arriveThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, destPos, speed * Time.deltaTime);
            yield return null;
        }

        // 到達後停留
        if (waitAtDestination > 0f) yield return new WaitForSeconds(waitAtDestination);
    }

    IEnumerator MoveLoop()
    {
        isMoving = true;
        Vector3 from = startPos;
        Vector3 to = destPos;
        bool goingToDest = true;

        while (true)
        {
            Vector3 target = goingToDest ? to : from;
            while (Vector3.Distance(transform.position, target) > arriveThreshold)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            // 到端點後等待
            if (waitAtEnds > 0f) yield return new WaitForSeconds(waitAtEnds);

            // 切換方向
            goingToDest = !goingToDest;
        }
    }

    void SetVisualsAndColliders(bool enabled)
    {
        // MeshRenderer / SpriteRenderer
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = enabled;

        // Colliders
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = enabled;
    }

    // 在編輯器或執行中更新目標位置（若 targetPoint 或 offset 被改變）
    void OnValidate()
    {
        if (Application.isPlaying) return;
        // 只在編輯器刷新顯示目的地位置
        startPos = transform.position;
        destPos = (targetPoints != null && targetPoints.Length > 0) 
            ? targetPoints[0].position 
            : startPos + targetOffset;
    }
}
