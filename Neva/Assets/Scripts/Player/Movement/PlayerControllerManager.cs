using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerControllerManager : MonoBehaviour
{
    public ScriptableStats stats;

    [Space]
    public List<PlayerMovement> playerMovementList = new List<PlayerMovement>();
    
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public CapsuleCollider2D capsule;

    private Vector2 velocity;
    private bool _cachedQueryStartInColliders;
    public bool grounded { private set; get; }
    public bool gravity { private set; get; }
    public Vector2 movementDirection { private set; get; }

    public event Action<bool, float> OnGroundedChanged;

    private void Awake()
    {
        gravity = true;

        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();

        movementDirection = Vector2.right;

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        foreach (var module in playerMovementList)
        {
            (module as IMovementModule).ModifyVelocity(ref velocity);
        }

        HandleGravity();

        rb.linearVelocity = velocity;
        
        if(velocity.x != 0)
            movementDirection = new Vector2(Mathf.Sign(velocity.x), 0f).normalized;
    }

    public void KillMomentum() => rb.linearVelocity = Vector2.zero;
    public void DisableGravity() => gravity = false;
    public void EnableGravity() => gravity = true;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(capsule.bounds.center, capsule.size, capsule.direction, 0, Vector2.down, stats.GrounderDistance, ~stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(capsule.bounds.center, capsule.size, capsule.direction, 0, Vector2.up, stats.GrounderDistance, ~stats.PlayerLayer);

        // Hit a Ceiling
        if (ceilingHit) velocity.y = Mathf.Min(0, velocity.y);

        // Landed on the Ground
        if (!grounded && groundHit)
        {
            grounded = true;
            OnGroundedChanged?.Invoke(true, Mathf.Abs(velocity.y));
        }
        // Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            OnGroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void HandleGravity()
    {
        if (!gravity) return;

        if (grounded && velocity.y <= 0f)
        {
            velocity.y = stats.GroundingForce;
        }
        else
        {
            velocity.y = Mathf.MoveTowards(velocity.y, -stats.MaxFallSpeed, stats.FallAcceleration * Time.fixedDeltaTime);
        }
    }

    [Header("Debug")]
    [SerializeField] private bool showVelocityGizmo = true;
    [SerializeField] private bool showDirectionGizmo = true;
    [SerializeField] private float velocityGizmoScale = 0.1f;

    private void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        // Ground check line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * stats.GrounderDistance);

        if (!showVelocityGizmo) return;

        // Velocity arrow
        Gizmos.color = Color.green;

        Vector3 start = transform.position;
        Vector3 scaledVelocity = (Vector3)velocity * velocityGizmoScale;
        Vector3 end = start + scaledVelocity;

        Gizmos.DrawLine(start, end);
        
        Vector3 right;
        Vector3 left;

        // Draw arrow head
        if (velocity.magnitude > 0.01f)
        {
            right = Quaternion.Euler(0, 0, 25) * -scaledVelocity.normalized * 0.2f;
            left = Quaternion.Euler(0, 0, -25) * -scaledVelocity.normalized * 0.2f;

            Gizmos.DrawLine(end, end + right);
            Gizmos.DrawLine(end, end + left);
        }

        if (!showDirectionGizmo || !capsule) return;
        
        // Velocity arrow
        Gizmos.color = Color.blue;

        start = capsule.bounds.center;
        end = (Vector2)start + movementDirection;

        Gizmos.DrawLine(start, end);

        right = Quaternion.Euler(0, 0, 25) * -movementDirection.normalized * 0.2f;
        left = Quaternion.Euler(0, 0, -25) * -movementDirection.normalized * 0.2f;

        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
}
