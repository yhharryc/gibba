using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class StateMachine : MonoBehaviour
{
    private State currentState;

    [SerializeField, InlineEditor]  // InlineEditor allows you to see and edit the ScriptableObject in the inspector
    private StateData stateData;  // Reference to the ScriptableObject containing the states

    // A list to hold instances of cloned states, shown in the inspector using Odin Inspector
    private List<State> instantiatedStates = new List<State>();

    private void Start()
    {
        InitializeStates();
        
        // Set the default state to the first cloned instance
        if (instantiatedStates.Count > 0)
        {
            currentState = instantiatedStates[0];
            currentState.InitializeState(this);
            currentState.Enter();
        }
    }

    // Initialize the states (clone the states from StateData)
    private void InitializeStates()
    {
        instantiatedStates.Clear();  // Clear existing states

        if (stateData != null)
        {
            foreach (State state in stateData.stateList)
            {
                State newStateInstance = state.CreateNewInstance(this);  // Clone each state
                instantiatedStates.Add(newStateInstance);
                newStateInstance.StateChangeRequested+=OnStateChangeRequested;
            }
        }
    }

    public void ChangeToState(State newState)
    {
        currentState.Exit();  // Exit the current state
        currentState = newState;  // Switch to the new state
        currentState.Enter();  // Enter the new state
    }

    public void Update()
    {
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    private void LateUpdate()
    {
        currentState?.LateUpdate();
    }

    // Method to change states based on a state name
    private void OnStateChangeRequested(string newStateName)
    {
        State newState = instantiatedStates.Find(state => state.GetType().Name == newStateName);

        if (newState != null)
        {
            ChangeToState(newState);
        }
        else
        {
            Debug.LogError($"State {newStateName} not found.");
        }
    }

    private void OnDestroy() {
        foreach (State state in instantiatedStates)
        {
            state.StateChangeRequested-=OnStateChangeRequested;
        }
    }
}
