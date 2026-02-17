using UnityEngine;

public abstract class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public PlayerControllerManager playerControllerManager;
    [HideInInspector]
    public PlayerInputSystem playerInput;
}

public interface IMovementModule
{
    void ModifyVelocity(ref Vector2 currentVelocity);
}