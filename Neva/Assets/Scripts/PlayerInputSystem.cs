using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    private InputSystem_Actions playerInput;

    public event Action OnPlayerJump;
    public event Action OnPlayerDodge;
    public event Action OnPlayerAttack;

    public event Action<Vector2> OnPlayerMove;

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

        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMove;
        playerInput.Player.Jump.performed += OnJump;
        playerInput.Player.Jump.canceled += OnJump;
        playerInput.Player.Dodge.performed += OnDodge;
        playerInput.Player.Dodge.canceled += OnDodge;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        OnPlayerMove?.Invoke(context.ReadValue<Vector2>());
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
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

        playerInput.Player.Move.performed -= OnMove;
        playerInput.Player.Move.canceled -= OnMove;
        playerInput.Player.Jump.performed -= OnJump;
        playerInput.Player.Jump.canceled -= OnJump;
        playerInput.Player.Dodge.performed -= OnDodge;
        playerInput.Player.Dodge.canceled -= OnDodge;
    }
}
