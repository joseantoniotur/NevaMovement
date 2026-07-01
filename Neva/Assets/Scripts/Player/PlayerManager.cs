using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public PlayerInputSystem playerInput;
    [HideInInspector]
    public PlayerControllerManager playerControllerManager;
    [HideInInspector]
    public PlayerAnimationManager playerAnimationManager;

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
        playerAnimationManager = GetComponent<PlayerAnimationManager>();
    }
}
