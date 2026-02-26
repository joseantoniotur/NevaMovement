using UnityEngine;

public class ClimbingMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;

    private Collider2D playerCollider;
    private RaycastHit2D currentHit;

    private Vector2 surfaceNormal;
    private Vector2 climbTangent;   // direction along the wall (up/down) – player's up after rotation
    private int climbSide;           // 1 = right side, -1 = left side (determined at grab)

    private float playerMove;
    private bool isClimbing;

    private float lastGrabTime;
    private float lastValidSurfaceTime;

    // Optional event for detachment (e.g., reached top)
    public System.Action OnDetached;

    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        playerCollider = playerControllerManager.capsule;
        stats = playerControllerManager.stats;

        if (playerInput)
        {
            playerInput.OnPlayerMove += OnMove;
            playerInput.OnPlayerJump += ExitClimbing;
            playerInput.OnPlayerDodge += ExitClimbing;
        }
    }

    void Update()
    {
        if (!isClimbing)
            TryStartClimbing();
    }

    private void OnMove(Vector2 input)
    {
        if (isClimbing)
            playerMove = input.y;
    }

    private void TryStartClimbing()
    {
        if (Time.time - lastGrabTime <= stats.grabCooldown)
            return;

        if (DetectSurface(out RaycastHit2D hit))
        {
            StartClimbing(hit);
        }
    }

    private void StartClimbing(RaycastHit2D hit)
    {
        isClimbing = true;

        currentHit = hit;
        surfaceNormal = hit.normal;

        // Determine which side of the wall we grabbed
        Vector2 playerToWall = hit.point - (Vector2)transform.position;
        climbSide = (int)Mathf.Sign(Vector2.Dot(playerToWall, transform.right));

        UpdateClimbTangent();

        playerControllerManager.KillMomentum();
        playerControllerManager.DisableGravity();

        SnapToSurface(); // includes rotation

        lastValidSurfaceTime = Time.time;
    }

    // Virtual so derived classes can add behaviour
    protected virtual void OnDetach()
    {
        OnDetached?.Invoke();
    }

    public void ExitClimbing()
    {
        if (!isClimbing)
            return;

        isClimbing = false;

        playerControllerManager.EnableGravity();
        transform.rotation = Quaternion.identity; // return to upright

        OnDetach();

        lastGrabTime = Time.time;
    }

    private void UpdateClimbTangent()
    {
        // Base tangent (perpendicular to normal)
        Vector2 baseTangent = new Vector2(-surfaceNormal.y, surfaceNormal.x);

        // Flip based on which side of the wall we're on so that +input moves "up" the wall
        climbTangent = baseTangent * climbSide;
    }

    public void ModifyVelocity(ref Vector2 velocity)
    {
        if (!isClimbing)
            return;

        if (!StillOnSurface())
        {
            if (Time.time - lastValidSurfaceTime > stats.surfaceGraceTime)
            {
                ExitClimbing();
                return;
            }
        }
        else
        {
            lastValidSurfaceTime = Time.time;
        }

        // Desired movement along the wall
        Vector2 desiredVelocity = Vector2.zero;
        if (Mathf.Abs(playerMove) > 0.01f)
        {
            desiredVelocity = climbTangent * (-playerMove * stats.climbSpeed * Time.fixedDeltaTime);
        }

        // Check if we can move in that direction
        Vector2 targetPosition = (Vector2)transform.position + desiredVelocity;
        RaycastHit2D forwardCheck = Physics2D.Raycast(targetPosition, -surfaceNormal, stats.rayDistance * 2f, stats.climbLayer);

        if (forwardCheck.collider != null)
        {
            // Path is clear – update surface data
            currentHit = forwardCheck;
            UpdateSurfaceData();
            velocity = desiredVelocity / Time.fixedDeltaTime;
        }
        else
        {
            // Blocked – look around curves
            Vector2[] checkDirections = new Vector2[]
            {
                climbTangent * 0.5f,
                (climbTangent + surfaceNormal).normalized * 0.5f,
                (climbTangent - surfaceNormal).normalized * 0.5f
            };

            bool foundPath = false;
            foreach (Vector2 dir in checkDirections)
            {
                Vector2 checkPos = (Vector2)transform.position + dir;
                RaycastHit2D cornerCheck = Physics2D.Raycast(checkPos, -surfaceNormal, stats.rayDistance * 2f, stats.climbLayer);

                if (cornerCheck.collider != null)
                {
                    currentHit = cornerCheck;
                    UpdateSurfaceData();
                    velocity = climbTangent * (playerMove * stats.climbSpeed);
                    foundPath = true;
                    break;
                }
            }

            if (!foundPath)
            {
                velocity = Vector2.zero;
            }
        }

        SnapToSurface();
    }

    private bool DetectSurface(out RaycastHit2D hit)
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 origin = bounds.center;
        Vector2 direction = playerControllerManager.movementDirection.normalized;

        hit = Physics2D.Raycast(origin, direction, stats.rayDistance, stats.climbLayer);
        return hit.collider != null;
    }

    private bool StillOnSurface()
    {
        Vector2 origin = playerCollider.bounds.center;
        RaycastHit2D hit = Physics2D.Raycast(origin, -surfaceNormal, stats.rayDistance * 1.5f, stats.climbLayer);

        if (hit.collider != null)
        {
            currentHit = hit;
            return true;
        }
        return false;
    }

    private void UpdateSurfaceData()
    {
        surfaceNormal = currentHit.normal;
        UpdateClimbTangent();
    }

    private void SnapToSurface()
    {
        if (currentHit.collider == null)
            return;

        Vector2 closestPoint = currentHit.collider.ClosestPoint(transform.position);
        Vector2 targetPosition = closestPoint + surfaceNormal * (stats.skinWidth);

        transform.position = targetPosition;

        RotateToSurface();
    }

    private void RotateToSurface()
    {
        float angle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + 180f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, stats.rotationSpeed * Time.fixedDeltaTime);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerCollider == null)
            return;

        Bounds bounds = playerCollider.bounds;
        Vector2 center = bounds.center;

        if (!isClimbing)
        {
            Vector2 moveDir = playerControllerManager != null ? playerControllerManager.movementDirection.normalized : Vector2.right;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(center, moveDir * stats.rayDistance);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(center, -surfaceNormal * stats.rayDistance * 1.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(center, surfaceNormal * 0.75f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(center, climbTangent * 0.75f);

            // Show player's up direction (should align with climbTangent)
            Gizmos.color = Color.white;
            Gizmos.DrawRay(center, transform.up * 0.5f);

            if (currentHit.collider != null)
            {
                Vector2 closestPoint = currentHit.collider.ClosestPoint(transform.position);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(closestPoint, 0.06f);

                float halfHeight = bounds.extents.y;
                Vector2 snapTarget = closestPoint + surfaceNormal * (halfHeight + stats.skinWidth);

                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(snapTarget, 0.06f);
                Gizmos.DrawLine(closestPoint, snapTarget);
            }
        }
    }
}