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

    private bool grounded;
    private bool isDodging = false;
    private Vector2 velocity;
    private bool _cachedQueryStartInColliders;

    public event Action<bool, float> GroundedChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start()
    {
        if (GetComponent<DodgeMovement>())
        {
            GetComponent<DodgeMovement>().StartDodge += StartDodge;
            GetComponent<DodgeMovement>().EndDodge += EndDodge;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        foreach (var module in playerMovementList)
        {
            if ((isDodging && module is DodgeMovement) || !isDodging)
            { 
                (module as IMovementModule).ModifyVelocity(ref velocity);
            }
        }

        HandleGravity();

        rb.linearVelocity = velocity;
    }

    public void KillMomentum() => rb.linearVelocity = Vector2.zero;
    private void StartDodge() => isDodging = true;
    private void EndDodge() => isDodging = false;

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
            GroundedChanged?.Invoke(true, Mathf.Abs(velocity.y));
        }
        // Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void HandleGravity()
    {
        if (isDodging) return;

        if (grounded && velocity.y <= 0f)
        {
            velocity.y = stats.GroundingForce;
        }
        else
        {
            velocity.y = Mathf.MoveTowards(velocity.y, -stats.MaxFallSpeed, stats.FallAcceleration * Time.fixedDeltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + Vector3.down * (stats.GrounderDistance));
    }
}
