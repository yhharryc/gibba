using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "State Machine/Odin State Data Asset")]
public class StateData : SerializedScriptableObject
{
    // Use ListDrawerSettings to make the list editable in the inspector
    [SerializeField, ShowInInspector, ListDrawerSettings(CustomAddFunction = "AddState", DraggableItems = false)]
    public List<State> stateList = new List<State>();

    // Define the default state
    public State DefaultState
    {
        get
        {
            if (stateList.Count > 0)
            {
                return stateList[0];
            }
            else
            {
                Debug.LogError("StateData has no states in the list");
                return null;
            }
        }
    }

    // This method is called when adding new states to the list via the inspector
    private State AddState()
    {
        // You can modify this method to dynamically create different types of states
        return new IdleState(null);  // Example: Add an IdleState
    }
}
