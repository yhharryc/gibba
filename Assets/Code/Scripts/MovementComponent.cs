using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // Add InputSystem namespace

public class MovementComponent : MonoBehaviour
{
    private Vector2 movementInput;
    public Vector2 MovementInput
    {
        get { return movementInput; }
        set { movementInput = value; }
    }

    [Header("Components")]
    private Rigidbody body;
    CharacterGround ground;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)] [Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to reach max speed")] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop after letting go")] public float maxDecceleration = 52f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
    [SerializeField] [Tooltip("Friction to apply against movement on stick")] private float friction;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;  // Custom gravity value
    private float verticalVelocity = 0f;

    [Header("Options")]
    [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")] public bool useAcceleration;
    public bool itsTheIntro = true;

    [Header("Calculations")]
    private Vector2 desiredVelocity;
    public Vector2 velocity;
    private float maxSpeedChange;
    private float acceleration;
    private float deceleration;
    private float turnSpeed;

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey;

    private void Awake()
    {
        // Find the character's Rigidbody and ground detection script
        body = GetComponent<Rigidbody>();
        ground = GetComponent<CharacterGround>();

        // Disable built-in gravity
        body.useGravity = false;
    }

    private void Update()
    {
        desiredVelocity = new Vector2(movementInput.x, 0f) * Mathf.Max(maxSpeed - friction, 0f);
    }

    private void FixedUpdate()
    {
        // FixedUpdate runs in sync with Unity's physics engine

        // Get Kit's current ground status from the ground detection script
        onGround = ground.GetOnGround();

        // Apply gravity manually
        ApplyGravity();

        // Get the Rigidbody's current velocity
        velocity = body.velocity;

        // Calculate movement, depending on whether "Instant Movement" has been checked
        if (useAcceleration)
        {
            runWithAcceleration();
        }
        else
        {
            if (onGround)
            {
                runWithoutAcceleration();
            }
            else
            {
                runWithAcceleration();
            }
        }
    }

    // Move Function that receives input from Player Input System
    public void Move(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        pressingKey = movementInput.sqrMagnitude > 0.01f;  // Check if any direction is pressed
    }

    private void ApplyGravity()
    {
        if (onGround)
        {
            verticalVelocity = 0f;  // Reset gravity if on the ground
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;  // Apply gravity over time
        }

        // Apply the vertical velocity to the Rigidbody (keeping horizontal velocity unchanged)
        body.velocity = new Vector3(body.velocity.x, verticalVelocity, body.velocity.z);
    }

    private void runWithAcceleration()
    {
        // Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground or in the air
        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDecceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        if (pressingKey)
        {
            // If the input direction's sign doesn't match our movement, we're turning and should use the turn speed stat.
            if (Mathf.Sign(movementInput.x) != Mathf.Sign(velocity.x))
            {
                maxSpeedChange = turnSpeed * Time.deltaTime;
            }
            else
            {
                // If we're simply running along, use the acceleration stat
                maxSpeedChange = acceleration * Time.deltaTime;
            }
        }
        else
        {
            // If not pressing a direction, use deceleration stat
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        // Move velocity towards the desired velocity, at the rate of maxSpeedChange
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);

        // Apply the new velocity to the Rigidbody
        body.velocity = new Vector3(velocity.x, verticalVelocity, body.velocity.z);
    }

    private void runWithoutAcceleration()
    {
        // If we're not using acceleration and deceleration, directly set the desired velocity (direction * max speed)
        velocity.x = desiredVelocity.x;

        // Apply the velocity to the Rigidbody
        body.velocity = new Vector3(velocity.x, verticalVelocity, body.velocity.z);
    }
}
