using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "State Machine/Odin State Data Asset")]
public class StateData : ScriptableObject
{
    [SerializeField]
    public List<State> stateList = new List<State>();  // Odin can serialize regular class instances

}
