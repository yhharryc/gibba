using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public abstract class OnGroundState<T> : PlayerState<T> where T : OnGroundState<T>
{
    public OnGroundState(StateMachine stateMachine) : base(stateMachine) { }
    
    public override void Enter()
    {
        
        base.Enter();  // Call base Enter method

        if (playerMovement == null)
        {
            playerMovement = stateMachine.gameObject.GetComponentInChildren<MovementComponent>();
        }

        // Hook the Move action to the Move method
        if (playerInput != null)
        {
            playerInput.actions["Move"].performed += Move;  // Subscribe to the Move input
            playerInput.actions["Move"].canceled += Move;   // Also handle canceled input to reset movement
            playerInput.actions["Jump"].started += Jump;  // Subscribe to the Move input
            
        }
        else
        {
            Debug.LogError("PlayerInput reference is missing in IdleState.");
        }
    }
        public override void Exit()
        {
            base.Exit();
            playerInput.actions["Move"].performed -= Move;  // Subscribe to the Move input
            playerInput.actions["Move"].canceled -= Move;   // Also handle canceled input to reset movement

            playerInput.actions["Jump"].started -= Jump;  // Subscribe to the Move input
        }  

        public void Move(InputAction.CallbackContext context)
        {
        Vector2 movementInput = context.ReadValue<Vector2>(); 
        playerMovement.MovementInput = movementInput;
        if(movementInput!=Vector2.zero)
        {
            RequestStateChange("MoveState");
        }
        }

        public void Jump(InputAction.CallbackContext context)
        {
        if (context.started)
        {
            RequestStateChange("JumpState");
        }
           
        }
    } 

