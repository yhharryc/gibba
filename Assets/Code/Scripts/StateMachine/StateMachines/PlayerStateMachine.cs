using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // Include the new Input System namespace
using UnityEngine.Events;      // To invoke UnityEvents

public class PlayerStateMachine : StateMachine
{
    private PlayerMovement playerMovement;
     // Expose the PlayerInput in the Inspector with Odin Inspector
    private PlayerInput playerInput;  // Reference to the PlayerInput component
    public PlayerInput PlayerInput{get{return playerInput;}}
    private void Awake()
    {
        // Try to find the PlayerMovement component in the current GameObject or its children
        playerMovement = GetComponentInChildren<PlayerMovement>();
        playerInput = GetComponentInChildren<PlayerInput>();
        if (playerMovement == null)
        {
            // If PlayerMovement is not found, search for any Rigidbody in this GameObject or its children
            Rigidbody foundRigidbody = GetComponentInChildren<Rigidbody>();

            if (foundRigidbody != null)
            {
                // Add the PlayerMovement component to the GameObject that has the Rigidbody
                playerMovement = foundRigidbody.gameObject.AddComponent<PlayerMovement>();
                Debug.Log("PlayerMovement component was missing and has been added to the GameObject with Rigidbody.");
            }
            else
            {
                // Raise an error if no Rigidbody was found
                Debug.LogError("No Rigidbody found in this GameObject or its children. Cannot add PlayerMovement.");
            }
        }
    }

    private void OnEnable()
    {
        // Enable the input system when the object is enabled
        //inputActions.Enable();
    }

    private void OnDisable()
    {
        // Disable the input system when the object is disabled
        //inputActions.Disable();
    }



}
