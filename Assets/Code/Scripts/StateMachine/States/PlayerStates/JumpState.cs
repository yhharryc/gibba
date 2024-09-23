using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public class JumpState : PlayerState<JumpState>
{
    public JumpState(StateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        jumpComponent.StartJumping();
        playerInput.actions["Jump"].canceled += OnCancelJump;
    }

    public override void Exit()
    {
        base.Exit();
        playerInput.actions["Jump"].canceled -= OnCancelJump;
    }

    public void OnCancelJump(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            
            jumpComponent.CancelJumping();
            RequestStateChange("IdleState");
        }
    }
    public override void Update()
    {
        if (playerMovement.onGround)
        {
            RequestStateChange("IdleState");
        }
    }
}
