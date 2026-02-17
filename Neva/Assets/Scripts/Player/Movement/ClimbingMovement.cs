using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ClimbingMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;

    private Collider2D playerCollider;
    private RaycastHit2D currentHit;

    private float playerMove;
    private bool isClimbing;

    private float lastGrabTime;

    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        playerCollider = playerControllerManager.capsule;

        if (playerInput)
        { 
            playerInput.OnPlayerMove += OnMove;
            playerInput.OnPlayerJump += ExitClimbing;
        }

        if (playerControllerManager)
            stats = playerControllerManager.stats;
    }
    private void Update()
    {
        CanStartClimbing();
    }

    private void OnMove(Vector2 playerMovement)
    {
        if (!isClimbing) return;
        playerMove = playerMovement.y;
    }

    public void CanStartClimbing()
    {
        if (Time.time - lastGrabTime < stats.grabCooldown || isClimbing) return;

        // Cast ray from the front of the player
        Vector2 rayOrigin = playerCollider.bounds.center;
        Vector2 direction = transform.right; // Assuming player faces right

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, stats.rayDistance, stats.climbLayer);
        Debug.DrawRay(rayOrigin, direction * stats.rayDistance, Color.green, 1f);

        if (hit.collider != null && hit.collider is EdgeCollider2D)
        {
            StartClimbing(hit);
        }
    }

    private void StartClimbing(RaycastHit2D hit)
    {
        isClimbing = true;
        currentHit = hit;

        playerControllerManager.KillMomentum();
        playerControllerManager.DisableGravity();

        lastGrabTime = Time.time;
    }

    public void ExitClimbing()
    {
        isClimbing = false;

        transform.rotation = Quaternion.identity;
        playerControllerManager.EnableGravity();
    }

    public void ModifyVelocity(ref Vector2 currentVelocity)
    {
        if (!isClimbing)
            return;

        if (playerMove == 0)
        {
            currentVelocity.y = Mathf.MoveTowards(currentVelocity.y, 0f, stats.GroundDeceleration * Time.fixedDeltaTime);
            currentVelocity.x = 0f;
        }
        else
        {
            currentVelocity.y = Mathf.MoveTowards(currentVelocity.y, playerMove * stats.climbSpeed, stats.Acceleration * Time.fixedDeltaTime);

            // Smoothly maintain position relative to climbable surface
            MaintainPositionOnSurface(ref currentVelocity);
        }

        // Prevent movement when at ledge
        if (CheckForLedge() && playerMove > 0)
        {
            currentVelocity.y = 0;
        }

        // Check if we lost contact with the surface
        if (!CheckClimbSurface() || playerControllerManager.grounded)
        {
            ExitClimbing();
        }
    }

    private void MaintainPositionOnSurface(ref Vector2 currentVelocity)
    {
        if (currentHit.collider == null) return;

        // Get the closest point on the edge collider
        Vector2 closestPoint = currentHit.collider.ClosestPoint(transform.position);

        // Calculate the desired distance from the wall
        float playerHalfWidth = playerCollider.bounds.extents.x;
        Vector2 wallNormal = (transform.position - (Vector3)closestPoint).normalized;

        // Calculate target position (player should be skinWidth away from wall)
        Vector2 targetPosition = closestPoint + (wallNormal * (playerHalfWidth + stats.skinWidth));

        // Calculate movement needed
        Vector2 movementNeeded = targetPosition - (Vector2)transform.position;

        // Don't teleport, smoothly move towards target position
        float maxMove = stats.maxSnapDistance * Time.fixedDeltaTime * 60f; // Scale by framerate
        Vector2 smoothMovement = Vector2.MoveTowards(Vector2.zero, movementNeeded, maxMove);

        // Apply smooth correction
        currentVelocity.x = smoothMovement.x / Time.fixedDeltaTime;
    }

    private bool CheckClimbSurface()
    {
        if (playerCollider == null) return false;

        // Cast multiple rays from different heights
        float playerHeight = playerCollider.bounds.size.y;
        float startY = -playerHeight * 0.4f;
        float endY = playerHeight * 0.4f;

        for (int i = 0; i < stats.horizontalRayCount; i++)
        {
            float t = i / (float)(stats.horizontalRayCount - 1);
            float yOffset = Mathf.Lerp(startY, endY, t);

            Vector2 rayOrigin = (Vector2)(transform.position + (transform.up * yOffset) + (transform.right * playerCollider.bounds.extents.x));
            Vector2 direction = -transform.right; // Ray towards the wall

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, stats.rayDistance * 2, stats.climbLayer);
            Debug.DrawRay(rayOrigin, direction * stats.rayDistance, isClimbing ? Color.cyan : Color.gray);

            if (hit.collider != null && hit.collider is EdgeCollider2D)
            {
                currentHit = hit;
                return true;
            }
        }

        return false;
    }

    private bool CheckForLedge()
    {
        // Cast ray upward to check for ledge above player
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * playerCollider.bounds.extents.y;
        Vector2 direction = transform.right;

        RaycastHit2D ledgeHit = Physics2D.Raycast(rayOrigin, direction, stats.ledgeCheckDistance, stats.climbLayer);

        return ledgeHit.collider == null; //Return true if there is not a ledge
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || playerCollider == null) return;
        
        Vector2 rayOrigin;

        // Draw climb detection rays
        Gizmos.color = isClimbing ? Color.green : Color.red;

        float playerHeight = playerCollider.bounds.size.y;
        float startY = -playerHeight * 0.4f;
        float endY = playerHeight * 0.4f;

        for (int i = 0; i < stats.horizontalRayCount; i++)
        {
            float t = i / (float)(stats.horizontalRayCount - 1);
            float yOffset = Mathf.Lerp(startY, endY, t);

            rayOrigin = (Vector2)(transform.position + (transform.up * yOffset) + (transform.right * playerCollider.bounds.extents.x));
            Gizmos.DrawRay(rayOrigin, -transform.right * stats.rayDistance);
        }

        //Ledge
        Gizmos.color = CheckForLedge() ? Color.white : Color.yellow;

        rayOrigin = (Vector2)transform.position + Vector2.up * playerCollider.bounds.extents.y;
        Vector2 direction = transform.right;
        Gizmos.DrawRay(rayOrigin, direction);

        // Draw target position if climbing
        if (isClimbing && currentHit.collider != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 closestPoint = currentHit.collider.ClosestPoint(transform.position);
            Gizmos.DrawSphere(closestPoint, 0.05f);
        }
    }
}