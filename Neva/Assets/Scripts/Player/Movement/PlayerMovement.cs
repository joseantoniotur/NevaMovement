using UnityEngine;

[System.Flags]
public enum MovementFlag
{
    None = 0,

    WALKING = 1 << 0,
    JUMPING = 1 << 1,
    DODGING = 1 << 2,
    CLIMBING = 1 << 3,

    All = ~0
}

public abstract class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public PlayerControllerManager playerControllerManager;
    [HideInInspector]
    public PlayerInputSystem playerInput;
    
    public MovementFlag movementsToAvoid;

    public bool CanPerformMovement(MovementFlag currentMovementFlags)
    {
        return (currentMovementFlags & movementsToAvoid) == 0; //Si no hay una flag que cuadre, podrá hacer el movimiento
    }
}

public interface IMovementModule
{
    void ModifyVelocity(ref Vector2 currentVelocity);
}