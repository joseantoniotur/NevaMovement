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

    public event Action StartDodge;
    public event Action EndDodge;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = PlayerManager.Instance.playerInput;
        playerControllerManager = PlayerManager.Instance.playerControllerManager;

        if (playerInput)
        {
            playerInput.OnPlayerDodge += OnDodge;
        }

        if (playerControllerManager)
        {
            playerControllerManager.GroundedChanged += Grounded;
            stats = playerControllerManager.stats;
        }
    }

    private void OnDodge()
    {
        dodgeToConsume = true;
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
            playerControllerManager.KillMomentum();
            currentVelocity = Vector2.zero;
            dodgeToConsume = false;
            
            if(grounded)
                currentDodgeCount = 0;

            EndDodge?.Invoke();
        }
    }

    private void ExecuteDodge(ref Vector2 currentVelocity)
    {
        playerControllerManager.KillMomentum();

        currentDodgeCount += 1;
        timeDodgeWasStarting = Time.time;

        currentVelocity.x = stats.DodgePower * transform.localScale.x;
        currentVelocity.y = 0f;
        StartDodge?.Invoke();
    }
}