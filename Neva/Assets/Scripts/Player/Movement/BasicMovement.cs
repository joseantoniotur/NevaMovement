using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;

    private bool grounded;
    private float playerMove;

    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        if (playerInput)
        {
            playerInput.OnStartPlayerMove += OnMoveStarted;
            playerInput.OnPlayerMove += OnMove;
            playerInput.OnEndPlayerMove += OnMoveCanceled;
        }

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged += Grounded;
            stats = playerControllerManager.stats;
        }
    }

    private void OnMoveStarted()
    {
        playerControllerManager.AddMovementFlag(MovementFlag.WALKING);
    }
    private void OnMove(Vector2 playerMovement)
    {
        playerMove = Mathf.Abs(playerMovement.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(playerMovement.x);
    }
    private void OnMoveCanceled()
    {
        playerControllerManager.RemoveMovementFlag(MovementFlag.WALKING);
    }

    private void Grounded(bool isGrounded, float verticalVelocity)
    {
        grounded = isGrounded;
    }

    public void ModifyVelocity(ref Vector2 currentVelocity)
    {
        if (playerMove == 0)
        {
            var deceleration = grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            currentVelocity.x = Mathf.MoveTowards(playerMove, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Sign(playerMove), 1, 1);
            currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, playerMove * stats.MaxSpeed, stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void OnDisable()
    {
        if (playerInput)
        {
            playerInput.OnStartPlayerMove -= OnMoveStarted;
            playerInput.OnPlayerMove -= OnMove;
            playerInput.OnEndPlayerMove -= OnMoveCanceled;
        }

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged -= Grounded;
        }
    }
}