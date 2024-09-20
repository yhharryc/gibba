using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
using Sirenix.OdinInspector;
using System;

[Serializable]
public abstract class State
{
    
    protected StateMachine stateMachine;
    
    // Constructor for the base state
    public State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    protected GameObject Owner => stateMachine.gameObject;

    public void InitializeState(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {

    }    
    public virtual void Exit()
    {

    }
    public virtual void HandleInput()
    {

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
}
