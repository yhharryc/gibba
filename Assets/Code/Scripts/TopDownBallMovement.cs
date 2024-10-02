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
    public float groundCheckLength = 0.2f;      // Radius for ground check
    public LayerMask groundLayer;               // Layer to detect the ground

    private Rigidbody rb;
    private Vector2 movementInput;              // Input vector for movement
    private Vector3 movementVelocity;           // Current velocity from movement
    private bool isGrounded;                    // Is the player grounded?

    private Vector3 slopeNormal;                // Store the current ground normal for slope adjustment
    public bool IsOnSlope
    {
        get
        {
            // Return the slope angle if grounded and the slope angle is greater than the threshold
            if (isGrounded && GetSlopeAngle() > 0.1f)
            {
                return true;
            }
            // Return 0 if not on a slope
            return false;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  // Disable Unity's built-in gravity, we'll handle our own
        rb.constraints = RigidbodyConstraints.FreezeRotation;  // Prevent rotation, so movement is smooth
    }

    void Update()
    {

    }
    void LateUpdate()
    {
        // Perform ground check every frame to see if the player is grounded

    }
    void FixedUpdate()
    {
        // Apply the player's movement based on input
        //MovePlayer();

        CheckGroundStatus();

        // Optional: Log the current slope angle for debugging
        if (isGrounded)
        {
            float slopeAngle = GetSlopeAngle();
            Debug.Log($"Current Slope Angle: {slopeAngle}");

            Vector3 downhillDirection = GetDownhillDirection();
            Debug.Log($"Downhill Direction: {downhillDirection}");
        }
        ApplyCustomGravity();
    }

    // Input callback from the Unity Input System
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();  // Get movement input as a Vector2
    }

    // Move the player using Rigidbody physics and project movement direction onto the slope
    void MovePlayer()
    {
        if (movementInput.magnitude > 0)
        {
            // Convert 2D input into 3D direction for XZ movement
            Vector3 inputDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

            // Adjust the input direction based on the slope
            Vector3 moveDirection = AdjustDirectionToSlope(inputDirection);

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

    
// New variable for custom angle adjustment
public float downhillGravityAngle = 10f;  // Angle in degrees to rotate the downhill direction

void ApplyCustomGravity()
{
    if (isGrounded && IsOnSlope)
    {
        // Get the downhill direction of the slope
        Vector3 downhillDirection = GetDownhillDirection();
        
        // Rotate the downhill direction by the specified angle
        Quaternion rotation = Quaternion.AngleAxis(downhillGravityAngle, Vector3.right);  // Rotate around the X axis
        Vector3 rotatedDownhillDirection = rotation * downhillDirection;

        // Project the gravity force onto the slope (in the rotated downhill direction)
        Vector3 slopeGravity = rotatedDownhillDirection * Mathf.Abs(customGravity);

        // Apply the projected gravity along the slope
        rb.velocity += slopeGravity * Time.fixedDeltaTime;

        Debug.Log($"Slope Gravity Applied (Rotated): {slopeGravity}");
        Debug.DrawLine(transform.position, transform.position + rotatedDownhillDirection * 1.5f, Color.red,2f);
    }
    else if (isGrounded)
    {

    }
    else
    {
        // Apply normal vertical gravity when not grounded
        rb.velocity += Vector3.up * customGravity * Time.fixedDeltaTime;
        Debug.Log($"Normal Gravity Applied: {customGravity}");
    }
}




// Check if the player is grounded using a raycast instead of a sphere check
void CheckGroundStatus()
{
    // Perform a raycast from the groundCheck position straight downwards
    RaycastHit hit;
    if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundCheckLength, groundLayer))
    {
        isGrounded = true;  // The player is grounded if the raycast hits something

        // Store the slope normal for slope adjustments
        slopeNormal = hit.normal;

        // Reset the vertical velocity to prevent small fall-off forces
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }
    else
    {
        isGrounded = false;  // The player is not grounded if the raycast misses
    }
}

    // Function to get the slope angle of the ground the player is currently on
    public float GetSlopeAngle()
    {
        RaycastHit hit;

        // Perform a raycast downwards to check the ground normal
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 1f, groundLayer))
        {
            // Calculate the angle between the ground normal and the world's up vector
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Return the slope angle, rounded to the nearest integer, but keeping the variable as a float
            return Mathf.Round(slopeAngle);
        }

        return 0f;  // Return 0 if no ground is detected
    }

    // Function to get the downhill direction of the slope
    public Vector3 GetDownhillDirection()
    {
        RaycastHit hit;

        // Perform a raycast downwards to check the ground normal
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 1f, groundLayer))
        {
            // Get the surface normal
            Vector3 groundNormal = hit.normal;

            // Calculate the downhill direction by projecting the normal onto the XZ plane
            Vector3 downhillDirection = Vector3.Cross(Vector3.Cross(Vector3.up, groundNormal), groundNormal);
            downhillDirection.Normalize();

            // Ensure the downhill direction is correct
            if (downhillDirection.y > 0) // Check if it's pointing upwards
            {
                downhillDirection = -downhillDirection; // Invert to point downhill
            }

            // Draw the debug line for downhill direction (1.5 units long)
            Debug.DrawLine(transform.position, transform.position + downhillDirection * 1.5f, Color.red);

            return downhillDirection;  // Return the normalized downhill direction
        }

        return Vector3.zero;  // Return zero vector if no ground is detected
    }


    // Adjusts the movement input direction according to the slope
    Vector3 AdjustDirectionToSlope(Vector3 inputDirection)
    {
        if (slopeNormal != Vector3.zero)
        {
            // Project the input direction onto the slope using the ground normal
            Vector3 slopeAdjustedDirection = Vector3.ProjectOnPlane(inputDirection, slopeNormal);

            // Draw a debug line from the player's position showing the original input direction in blue
            Debug.DrawLine(transform.position, transform.position + inputDirection * 2f, Color.blue);

            // Draw a debug line from the player's position showing the slope-adjusted direction in green
            Debug.DrawLine(transform.position, transform.position + slopeAdjustedDirection * 2f, Color.green);

            return slopeAdjustedDirection.normalized;
        }

        // If no slope normal is detected, return the original input direction and draw it
        Debug.DrawLine(transform.position, transform.position + inputDirection * 2f, Color.blue);
        return inputDirection;
    }



    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;

            // Draw a line to visualize the raycast
            Vector3 raycastEndPoint = groundCheck.position + Vector3.down * groundCheckLength; // 1f is the distance of the raycast
            Gizmos.DrawLine(groundCheck.position, raycastEndPoint);

        }
    }

}
