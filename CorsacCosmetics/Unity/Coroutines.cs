using UnityEngine;

namespace CorsacCosmetics.Unity;

public class Coroutines : MonoBehaviour
{
    public static Coroutines Instance { get; private set; } = null!;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}