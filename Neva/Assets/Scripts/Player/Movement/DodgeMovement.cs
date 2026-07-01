using System;
using UnityEngine;

public class DodgeMovement : PlayerMovement, IMovementModule
{
    private ScriptableStats stats;

    private bool grounded;

    private bool dodgeToConsume;
    private int currentDodgeCount;
    private float timeDodgeWasStarting;
    private bool HasFinishedDodge => Time.time > timeDodgeWasStarting + stats.DodgeDuration;
    private bool CanDodge => currentDodgeCount < stats.MaxDodgeCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        if (playerInput)
        {
            playerInput.OnPlayerDodge += OnDodge;
            playerInput.OnPlayerJump += OnJump;
        }

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged += Grounded;
            stats = playerControllerManager.stats;
        }
    }

    private void OnDodge()
    {
        dodgeToConsume = true;
    }

    private void OnJump()
    {
        EndDodge();
    }

    private void Grounded(bool isGrounded, float verticalVelocity)
    {
        grounded = isGrounded;
        if (grounded)
            currentDodgeCount = 0;
    }

    public void ModifyVelocity(ref Vector2 currentVelocity)
    {
        if (dodgeToConsume && CanDodge && HasFinishedDodge) ExecuteDodge(ref currentVelocity);

        if (HasFinishedDodge && dodgeToConsume)
        {
            currentVelocity = Vector2.zero;
            EndDodge();
        }
    }

    private void ExecuteDodge(ref Vector2 currentVelocity)
    {
        playerControllerManager.KillMomentum();

        currentDodgeCount += 1;
        timeDodgeWasStarting = Time.time;

        currentVelocity.x = stats.DodgePower * transform.localScale.x;
        currentVelocity.y = 0f;

        playerControllerManager.AddMovementFlag(MovementFlag.DODGING);
    }

    private void EndDodge()
    {
        playerControllerManager.KillMomentum();
        dodgeToConsume = false;

        if (grounded)
            currentDodgeCount = 0;

        playerControllerManager.RemoveMovementFlag(MovementFlag.DODGING);
    }

    private void OnDisable()
    {
        if (playerInput)
        {
            playerInput.OnPlayerDodge -= OnDodge;
            playerInput.OnPlayerJump -= OnJump;
        }

        if (playerControllerManager)
        {
            playerControllerManager.OnGroundedChanged -= Grounded;
        }
    }
}