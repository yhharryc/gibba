using UnityEngine;
using UnityEngine.InputSystem;  // For Input System integration

[RequireComponent(typeof(Rigidbody))]
public class TopDownBallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxMovementSpeed = 10f;        // Maximum movement speed
    public float acceleration = 5f;             // How quickly the player reaches max speed
    public float deceleration = 2f;             // How quickly the player slows down when no input

    [Header("Custom Gravity")]
    public float customGravity = -9.81f;        // Custom gravity force

    [Header("Ground Detection")]
    public Transform groundCheck;               // A point at the bottom to detect ground
    public float groundCheckRadius = 0.2f;      // Radius for ground check
    public LayerMask groundLayer;               // Layer to detect the ground

    private Rigidbody rb;
    private Vector2 movementInput;              // Input vector for movement
    private Vector3 movementVelocity;           // Current velocity from movement
    private bool isGrounded;                    // Is the player grounded?

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  // Disable Unity's built-in gravity, we'll handle our own
        rb.constraints = RigidbodyConstraints.FreezeRotation;  // Prevent rotation, so movement is smooth
    }

    void Update()
    {
        // Perform ground check every frame to see if the player is grounded
        CheckGroundStatus();
    }

    void FixedUpdate()
    {
        // Apply the player's movement based on input
        MovePlayer();
        
        // Apply custom gravity when not grounded
        if (!isGrounded)
        {
            ApplyCustomGravity();
        }
    }

    // Input callback from the Unity Input System
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();  // Get movement input as a Vector2
    }

    // Move the player using Rigidbody physics
    void MovePlayer()
    {
        // If the player is providing movement input
        if (movementInput.magnitude > 0)
        {
            // Convert 2D input into 3D direction for XZ movement
            Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

            // Calculate the target velocity based on the movement direction
            Vector3 targetVelocity = moveDirection * maxMovementSpeed;

            // Smoothly transition to the target velocity using acceleration
            movementVelocity = Vector3.MoveTowards(movementVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate when no input is provided
            movementVelocity = Vector3.MoveTowards(movementVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        // Apply the movement velocity to the rigidbody's position
        rb.velocity = new Vector3(movementVelocity.x, rb.velocity.y, movementVelocity.z);
    }

    // Apply custom gravity when the player is not grounded
    void ApplyCustomGravity()
    {
        // Apply gravity to the Y-axis velocity
        rb.velocity += Vector3.up * customGravity * Time.fixedDeltaTime;
    }

    // Check if the player is grounded using a sphere overlap at the groundCheck position
    void CheckGroundStatus()
    {
        // Perform a sphere check to see if the player is touching the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Reset the vertical velocity if grounded to avoid small fall-off forces
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
    }

    // Debugging: Draw a sphere in the editor to visualize the ground check area
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
