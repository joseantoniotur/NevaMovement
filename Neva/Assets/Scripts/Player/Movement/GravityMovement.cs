using UnityEngine;

public class GravityMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;
    private bool grounded;

    void Start()
    {
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged += Grounded;
            stats = playerControllerManager.stats;
        }
    }

    public void ModifyVelocity(ref Vector2 currentVelocity)
    {
        if (grounded && currentVelocity.y <= 0f)
        {
            currentVelocity.y = stats.GroundingForce;
        }
        else
        {
            currentVelocity.y = Mathf.MoveTowards(currentVelocity.y, -stats.MaxFallSpeed, stats.FallAcceleration * Time.fixedDeltaTime);
        }
    }

    private void Grounded(bool isGrounded, float verticalVelocity)
    {
        grounded = isGrounded;
    }

    private void OnDisable()
    {
        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged -= Grounded;
        }
    }
}
