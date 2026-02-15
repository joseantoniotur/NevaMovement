using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public PlayerInputSystem playerInput;
    [HideInInspector]
    public PlayerControllerManager playerControllerManager;

    public static PlayerManager Instance { get; private set; }

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInput = GetComponent<PlayerInputSystem>();
        playerControllerManager = GetComponent<PlayerControllerManager>();
    }
}
