using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    private InputSystem_Actions playerInput;

    public event Action OnPlayerJump;

    public event Action OnPlayerDodge;
    public event Action OnPlayerAttack;

    public event Action OnStartPlayerMove;
    public event Action<Vector2> OnPlayerMove;
    public event Action OnEndPlayerMove;

    public static PlayerInputSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInput = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        playerInput.Enable();

        playerInput.Player.Move.started += OnStartMove;
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnEndMove;

        playerInput.Player.Jump.started += OnStartJump;

        playerInput.Player.Dodge.performed += OnDodge;
        playerInput.Player.Dodge.canceled += OnDodge;
    }

    private void OnStartMove(InputAction.CallbackContext context)
    {
        OnPlayerMove?.Invoke(context.ReadValue<Vector2>());
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        OnPlayerMove?.Invoke(context.ReadValue<Vector2>());
    }
    private void OnEndMove(InputAction.CallbackContext context)
    {
        OnPlayerMove?.Invoke(context.ReadValue<Vector2>());
    }

    private void OnStartJump(InputAction.CallbackContext context)
    {
        OnPlayerJump?.Invoke();
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnPlayerDodge?.Invoke();
    }

    private void OnDisable()
    {
        playerInput.Disable();

        playerInput.Player.Move.started -= OnStartMove;
        playerInput.Player.Move.performed -= OnMove;
        playerInput.Player.Move.canceled -= OnEndMove;

        playerInput.Player.Jump.started -= OnStartJump;

        playerInput.Player.Dodge.performed -= OnDodge;
        playerInput.Player.Dodge.canceled -= OnDodge;
    }
}
