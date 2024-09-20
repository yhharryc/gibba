using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class StateMachine : MonoBehaviour
{
    //[SerializeReference]
    //private State defaultState;
    private State currentState;

    [SerializeField]
    private StateData stateData;
    private void Start() {
        currentState = stateData.DefaultState;
        currentState.InitializeState(this);
        currentState.Enter();
    }
    public void ChangeToState(State newState)
    {
        currentState.Exit();  // Exit the current state
        currentState = newState;  // Switch to the new state
        currentState.Enter();  // Enter the new state
    }


    public void Update()
    {
        currentState.Update();
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdate();
    }

    private void LateUpdate()
    {
        currentState.LateUpdate();
    }


}
