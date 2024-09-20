using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
using System;

public class IdleState : PlayerState<IdleState>
{
    PlayerMovement playerMovement;
    public IdleState(StateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        
        base.Enter();  // Call base Enter method

        if (playerMovement == null)
        {
            playerMovement = Owner.GetComponentInChildren<PlayerMovement>();
        }

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
        if(movementInput!=Vector2.zero)
        {
            Debug.Log("YES");
        }
    }

    // Factory method to create a new instance of the specific state
    //public override State CreateNewInstance(StateMachine stateMachine)
    //{
    //    return new IdleState(stateMachine);  // Return a new instance
    //}
}
