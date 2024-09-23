using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public class MoveState : OnGroundState<MoveState>
{
    public MoveState(StateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        // Hook the Move action to the Move method
        if (playerInput != null)
        {
            playerInput.actions["Move"].performed += Move;  // Subscribe to the Move input
            playerInput.actions["Move"].canceled += Move;   // Also handle canceled input to reset movement
        }
        else
        {
            Debug.LogError("PlayerInput reference is missing in IdleState.");
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>(); 
        playerMovement.MovementInput = movementInput;
        if(movementInput==Vector2.zero)
        {
            RequestStateChange("IdleState");
        }
    }

    public override void Exit()
    {
        playerInput.actions["Move"].performed -= Move;  // Subscribe to the Move input
        playerInput.actions["Move"].canceled -= Move;   // Also handle canceled input to reset movement
    }
}
