using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
using System;


public abstract class State
{
    
    protected StateMachine stateMachine;

    // Event that will notify the state machine about a state change request
    public event Action<string> StateChangeRequested;
    
    // Constructor for the base state
    public State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }


    public void InitializeState(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        Debug.Log($"{this.GetType().Name} entered on {stateMachine.gameObject.name}.");
    }

    // Exit method with debug log
    public virtual void Exit()
    {
        Debug.Log($"{this.GetType().Name} exited on {stateMachine.gameObject.name}.");
    }

    public virtual void Update()
    {

    }
    public virtual void FixedUpdate()
    {

    }
    public virtual void LateUpdate() 
    {
        
    }
    // Factory method to create a new instance of the specific state
    public abstract State CreateNewInstance(StateMachine stateMachine);

    protected void RequestStateChange(string newStateName)
    {
        StateChangeRequested?.Invoke(newStateName);
    }
}
