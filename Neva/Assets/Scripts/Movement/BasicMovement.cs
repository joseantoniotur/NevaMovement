using UnityEngine;

public class BasicMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;

    private bool grounded;
    private float playerMove;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        if (playerInput)
        {
            playerInput.OnPlayerMove += OnMove;
        }

        if (playerControllerManager)
        {
            playerControllerManager.GroundedChanged += Grounded;
            stats = playerControllerManager.stats;
        }
    }

    private void OnMove(Vector2 playerMovement)
    {
        playerMove = Mathf.Abs(playerMovement.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(playerMovement.x);

        if (playerMove != 0)
            transform.localScale = new Vector3(Mathf.Sign(playerMove), 1, 1);
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
            currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, playerMove * stats.MaxSpeed, stats.Acceleration * Time.fixedDeltaTime);
        }
    }
}