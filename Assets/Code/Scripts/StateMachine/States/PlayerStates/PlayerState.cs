using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.InputSystem; 
[Serializable]

public abstract class PlayerState : State
{
    protected PlayerInput playerInput;
    public PlayerState(StateMachine stateMachine) : base(stateMachine) {
        // Check if the state machine is a PlayerStateMachine and get the PlayerInput
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
}
