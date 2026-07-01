using UnityEngine;

public class JumpMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;

    private bool grounded;
    private float frameLeftGrounded = float.MinValue;

    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool coyoteUsable;
    private float timeJumpWasPressed;

    private int currentJumpCount;

    private bool HasBufferedJump => bufferedJumpUsable && Time.time < timeJumpWasPressed + stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && Time.time < frameLeftGrounded + stats.CoyoteTime;
    private bool CanJump => currentJumpCount < stats.MaxJumpCount;

    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        if (playerInput)
        {
            playerInput.OnPlayerJump += OnStartJump;
            playerInput.OnPlayerDodge += FinishJump;
        }

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged += Grounded;
            stats = playerControllerManager.stats;
        }
    }

    private void OnStartJump()
    {
        jumpToConsume = true;
        timeJumpWasPressed = Time.time;
    }

    private void Grounded(bool isGrounded, float verticalVelocity)
    {
        grounded = isGrounded;
        if (grounded)
        {
            FinishJump();
            currentJumpCount = 0;
        }
        else
        {
            frameLeftGrounded = Time.time;
        }
    }

    private void FinishJump()
    {
        playerControllerManager.RemoveMovementFlag(MovementFlag.JUMPING);
    }

    public void ModifyVelocity(ref Vector2 currentVelocity)
    {
        if ((!jumpToConsume && !HasBufferedJump) || (!jumpToConsume && !CanJump)) return;

        if (grounded || CanUseCoyote || CanJump) ExecuteJump(ref currentVelocity);

        jumpToConsume = false;
    }

    private void ExecuteJump(ref Vector2 currentVelocity)
    {
        currentJumpCount += 1;
        
        timeJumpWasPressed = Time.time;
        
        bufferedJumpUsable = false;
        coyoteUsable = false;
        
        currentVelocity.y = stats.JumpPower;

        playerControllerManager.AddMovementFlag(MovementFlag.JUMPING);
    }

    private void OnDisable()
    {
        if (playerInput)
        {
            playerInput.OnPlayerJump -= OnStartJump;
            playerInput.OnPlayerDodge -= FinishJump;
        }

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged -= Grounded;
        }
    }
}