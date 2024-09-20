using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "State Machine/Odin State Data Asset")]
public class StateData : SerializedScriptableObject
{
    [ShowInInspector]
    public List<State> stateList = new List<State>();  // Odin can serialize regular class instances
    
    public State DefaultState {get{if(stateList.Count>0){return stateList[0];}else{Debug.LogError("StateData has no list for states");return null;}}}
}
