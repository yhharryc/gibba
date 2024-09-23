using UnityEngine;
using UnityEngine.InputSystem;

public class JumpComponent : MonoBehaviour
{
    [Header("Components")]
    [HideInInspector] public Rigidbody body;
    private CharacterGround ground;
    private GameFeelComponent juice;

    [Header("Jumping Stats")]
    [SerializeField, Range(2f, 5.5f)] public float jumpHeight = 7.3f;
    [SerializeField, Range(0.2f, 1.25f)] public float timeToJumpApex = 0.4f;
    [SerializeField, Range(0f, 5f)] public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)] public float downwardMovementMultiplier = 6.17f;
    [SerializeField, Range(0, 1)] public int maxAirJumps = 0;

    [Header("Options")]
    public bool variableJumpHeight = true;
    [SerializeField, Range(1f, 10f)] public float jumpCutOff = 3f;
    [SerializeField] public float speedLimit = 20f;
    [SerializeField, Range(0f, 0.3f)] public float coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)] public float jumpBuffer = 0.15f;

    [Header("Current State")]
    public bool canJumpAgain = false;
    private bool desiredJump;
    private bool pressingJump;
    public bool onGround;
    private bool currentlyJumping;

    private float jumpSpeed;
    private float calculatedGravity;
    private float defaultGravity;
    private float gravityMultiplier;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        ground = GetComponent<CharacterGround>();
        juice = GetComponentInChildren<GameFeelComponent>();

        // Calculate the required jump speed to reach the desired jump height in the given time
        calculatedGravity = (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        jumpSpeed = Mathf.Abs(calculatedGravity) * timeToJumpApex;

        // Store the default gravity (from Unity physics settings)
        defaultGravity = Physics.gravity.y;
    }
    public void StartJumping()
    {
        desiredJump = true;
        pressingJump = true;
        DoAJump();
        
    }

    public void CancelJumping()
    {
        pressingJump = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            desiredJump = true;
            pressingJump = true;
        }
        if (context.canceled)
        {
            pressingJump = false;
        }
    }

    private void Update()
    {
        onGround = ground.GetOnGround();
        HandleJumpBuffer();
        HandleCoyoteTime();
    }

    private void FixedUpdate()
    {
        ApplyAdditionalGravity();

        //if (desiredJump)
        //{
        //    DoAJump();
        //}

        // Clamp the fall velocity to the defined speed limit (terminal velocity)
        body.velocity = new Vector3(body.velocity.x, Mathf.Clamp(body.velocity.y, -speedLimit, 100));
    }

    private void ApplyAdditionalGravity()
    {
        float desiredGravity = 0f;

        // Determine the gravity multiplier depending on the velocity (upwards, downwards, or idle)
        if (body.velocity.y > 0.01f)  // Going up
        {
            if (pressingJump && currentlyJumping && variableJumpHeight)
            {
                // Apply upward movement multiplier while the player is holding the jump button
                desiredGravity = calculatedGravity * upwardMovementMultiplier;
            }
            else
            {
                // Apply jump cutoff gravity when the jump button is released
                desiredGravity = calculatedGravity * jumpCutOff;
            }
        }
        else if (body.velocity.y < -0.01f)  // Falling down
        {
            // Apply stronger gravity when falling
            desiredGravity = calculatedGravity * downwardMovementMultiplier;
        }
        else
        {
            // Neutral gravity when on the ground or not moving vertically
            desiredGravity = calculatedGravity;
        }

        // Calculate the difference between desired gravity and Unity's default gravity
        float gravityDifference = desiredGravity - defaultGravity;

        // Apply additional force to compensate for the difference in gravity
        body.AddForce(new Vector3(0, gravityDifference * body.mass, 0), ForceMode.Acceleration);
    }

    private void DoAJump()
    {
        if (onGround || canJumpAgain)
        {
            desiredJump = false;
            currentlyJumping = true;

            // Handle air jump if allowed
            canJumpAgain = maxAirJumps > 0 && !canJumpAgain;

            // Apply the initial jump velocity
            Vector3 velocity = body.velocity;
            velocity.y = jumpSpeed;
            body.velocity = velocity;

            // Apply jump effects (if any)
            if (juice != null)
            {
                juice.jumpEffects();
            }
        }
    }

    private void HandleJumpBuffer()
    {
        if (desiredJump && jumpBuffer > 0)
        {
            desiredJump = false;
        }
    }

    private void HandleCoyoteTime()
    {
        if (!onGround && !currentlyJumping)
        {
            canJumpAgain = coyoteTime > 0;
        }
    }
}
