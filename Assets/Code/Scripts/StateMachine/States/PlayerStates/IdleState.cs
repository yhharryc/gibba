using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public class IdleState : PlayerState
{
    PlayerMovement playerMovement;

    public override void Enter()
    {
        if(playerMovement==null)
        {
            playerMovement = Owner.GetComponentInChildren<PlayerMovement>();
        }
        
    }  
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>(); 
        playerMovement.MovementInput = movementInput;
        if(movementInput!=Vector2.zero)
        {
            Debug.Log("LOOL");
        }
    }
}
