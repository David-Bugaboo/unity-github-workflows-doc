using UnityEngine;
using UnityEngine.InputSystem;

public class BugaPlayerInput : MonoBehaviour
{
    [Header("----- Character Input Values -----")]
    public Vector2 move;
    public Vector2 look;
    public bool interact;
    public bool jump;
    public bool sprint;
    private bool forceSprint;

    [Header("----- Movement Setting -----")]
    public bool analogMovement;
    [Tooltip("Define a partir de qual magnitude do input o personagem começa a correr.")]
    [SerializeField] private float sprintThreshold = 0.9f;

    [Header("----- Mouse Cursor Settings -----")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;


    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    // public void OnLook(InputValue value)
    // {
    //     if (cursorInputForLook)
    //     {
    //         LookInput(value.Get<Vector2>());
    //     }
    // }

    public void OnInteract(InputValue value)
    {
        InteractInput(value.isPressed);
    }
    
    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
        if(!forceSprint) sprint = move.magnitude > sprintThreshold;
        else sprint = true;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void InteractInput(bool newInteractState)
    {
        interact = newInteractState;
    }
    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        forceSprint = newSprintState;
        sprint = newSprintState;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}

