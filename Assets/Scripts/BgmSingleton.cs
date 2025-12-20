using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BgmSingleton : MonoBehaviour
{
    public static BgmSingleton Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
