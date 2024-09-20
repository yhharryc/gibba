using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public abstract class State
{

    protected StateMachine stateMachine;
    
    protected GameObject Owner{
        get{return stateMachine.gameObject;}
    }

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
}
