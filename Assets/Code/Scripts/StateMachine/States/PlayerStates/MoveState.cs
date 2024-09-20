using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : PlayerState<MoveState>
{
    public MoveState(StateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        
    }
}
