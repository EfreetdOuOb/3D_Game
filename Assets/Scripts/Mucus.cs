using UnityEngine;

public class Mucus : MonoBehaviour
{
    [Header("踩到減速倍率 (0~1，越小越慢)")]
    [Range(0.01f, 1f)] public float slowMultiplier = 0.3f;
    [Header("禁止跳躍 (Player需public bool canJump)")]
    public bool forbidJump = false;
    [Header("接觸時地板顏色")] public Color mucusColor = Color.green;
    [Header("離開後debuff持續(秒)")]
    public float lingerDuration = 0f;
    [Header("粒子特效 (可自訂來源點＆方向)")]
    public ParticleSystem mucusParticle;

    private System.Collections.Generic.Dictionary<GameObject, float> playerOrigSpeed = new();
    private System.Collections.Generic.Dictionary<GameObject, bool> playerOrigJump = new();
    private System.Collections.Generic.Dictionary<GameObject, float> playerLingerTimer = new();
    private Material originalMaterial;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend) originalMaterial = rend.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var move = other.GetComponent<PlayerMove>();
        if (move != null && !playerOrigSpeed.ContainsKey(other.gameObject))
        {
            playerOrigSpeed[other.gameObject] = move.moveSpeed; //存速
            move.moveSpeed *= slowMultiplier;
            if (forbidJump && !playerOrigJump.ContainsKey(other.gameObject))
            {
                playerOrigJump[other.gameObject] = move.canJump;
                move.canJump = false;
            }
            if (rend) rend.material.color = mucusColor;
            if (mucusParticle) mucusParticle.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var move = other.GetComponent<PlayerMove>();
        if (move != null && playerOrigSpeed.ContainsKey(other.gameObject))
        {
            if (lingerDuration > 0f)
            {
                playerLingerTimer[other.gameObject] = lingerDuration;
            }
            else
            {
                move.moveSpeed = playerOrigSpeed[other.gameObject];
                playerOrigSpeed.Remove(other.gameObject);
                if (forbidJump && playerOrigJump.ContainsKey(other.gameObject))
                {
                    move.canJump = playerOrigJump[other.gameObject];
                    playerOrigJump.Remove(other.gameObject);
                }
                if (rend && originalMaterial != null) rend.material.color = originalMaterial.color;
            }
        }
    }

    private void Update()
    {
        // 處理debuff殘留
        if (playerLingerTimer.Count > 0)
        {
            var keys = new System.Collections.Generic.List<GameObject>(playerLingerTimer.Keys);
            foreach (var go in keys)
            {
                playerLingerTimer[go] -= Time.deltaTime;
                if (playerLingerTimer[go] <= 0f)
                {
                    var move = go.GetComponent<PlayerMove>();
                    if (move != null && playerOrigSpeed.ContainsKey(go))
                    {
                        move.moveSpeed = playerOrigSpeed[go];
                        playerOrigSpeed.Remove(go);
                    }
                    if (forbidJump && playerOrigJump.ContainsKey(go))
                    {
                        if (move != null) move.canJump = playerOrigJump[go];
                        playerOrigJump.Remove(go);
                    }
                    if (rend && originalMaterial != null) rend.material.color = originalMaterial.color;
                    playerLingerTimer.Remove(go);
                }
            }
        }
    }
}
// 用法說明：
// 將此腳本掛在地板物件，調整參數。粒子特效用Prefab拖到mucusParticle欄位，
// 預設Progression Space=Local，將特效物件位置設在你想要的發射點即可。
