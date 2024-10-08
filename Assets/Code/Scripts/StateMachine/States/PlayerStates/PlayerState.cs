using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.InputSystem; 
[Serializable]

public abstract class PlayerState<T> : State where T : PlayerState<T>
{
    protected PlayerInput playerInput;
    protected MovementComponent playerMovement;
    protected JumpComponent jumpComponent;

    public PlayerState(StateMachine stateMachine) : base(stateMachine) {
        // Check if the state machine is a PlayerStateMachine and get the PlayerInput
        this.stateMachine = stateMachine;
        if (stateMachine is PlayerStateMachine playerStateMachine)
        {
            Debug.Log("PlayerStateMachine detected in PlayerState.");
            playerInput = playerStateMachine.PlayerInput;  // Get the PlayerInput reference

            if (playerInput != null)
            {
                Debug.Log("PlayerInput has been successfully referenced in PlayerState.");
            }
            else
            {
                Debug.LogError("PlayerInput reference is missing in PlayerState.");
            }
        }

     }

    public override void Enter()
    {
        
        base.Enter();  // Call base Enter method
        if (playerMovement == null)
        {
            playerMovement = stateMachine.gameObject.GetComponentInChildren<MovementComponent>();
        }
        if(jumpComponent==null)
        {
            jumpComponent = stateMachine.gameObject.GetComponent<JumpComponent>();
            if(jumpComponent==null)
            {
                jumpComponent = stateMachine.gameObject.AddComponent<JumpComponent>();
                Debug.Log("Did not find Jump Component on character. Added one. ");
            }
        }  

    }

    public override State CreateNewInstance(StateMachine stateMachine)
    {
        // Pass the stateMachine argument to the constructor
        return Activator.CreateInstance(typeof(T), new object[] { stateMachine }) as State;
    }
}
