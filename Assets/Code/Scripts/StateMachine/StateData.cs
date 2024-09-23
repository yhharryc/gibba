using System;
using System.Collections.Generic;
using System.Linq;
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

    // Dropdown to select the type of state to add
    [ValueDropdown("GetAllStateTypes")]
    public Type stateToAdd;

    // Use reflection to get all non-abstract types derived from State
    private IEnumerable<ValueDropdownItem> GetAllStateTypes()
    {
        var stateType = typeof(State);
        var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(stateType));

        foreach (var type in derivedTypes)
        {
            yield return new ValueDropdownItem(type.Name, type);
        }
    }

    // This method is called when adding new states to the list via the inspector
    private State AddState()
    {
        if (stateToAdd != null)
        {
            // Dynamically create an instance of the selected state type
            State newState = Activator.CreateInstance(stateToAdd, new object[] { null }) as State;

            if (newState != null)
            {
                stateList.Add(newState);
                return newState;
            }
        }

        Debug.LogError("Failed to add state.");
        return null;
    }

    // Validation method to remove duplicate states
    private void OnValidate()
    {
        if (stateList == null || stateList.Count == 0) return;

        // Remove duplicates by grouping by the state type and keeping only one of each type
        stateList = stateList
            .GroupBy(state => state.GetType())  // Group by state type
            .Select(group => group.First())     // Keep only the first instance of each type
            .ToList();

        //Debug.Log("Duplicate states have been removed.");
    }

}
