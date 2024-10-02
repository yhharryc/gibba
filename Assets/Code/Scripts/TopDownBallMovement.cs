using UnityEngine;
using UnityEngine.InputSystem;  // For Input System integration

[RequireComponent(typeof(Rigidbody))]
public class TopDownBallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxMovementSpeed = 10f;        // Maximum movement speed
    public float maxAdditionalSpeed = 10f;  
    public float acceleration = 5f;             // How quickly the player reaches max speed
    public float deceleration = 2f;             // How quickly the player slows down when no input

    [Header("Custom Gravity")]
    public float customGravity = -9.81f;        // Custom gravity force
    public float slopeGravityMultiplier = 1.0f; // Multiplier for slope-based gravity

    [Header("Ground Detection")]
    public Transform groundCheck;               // A point at the bottom to detect ground
    public float groundCheckLength = 0.2f;      
    public LayerMask groundLayer;               // Layer to detect the ground

    private Rigidbody rb;
    private Vector2 movementInput;              // Input vector for movement
    private Vector3 movementVelocity;           // Current velocity from movement
    private Vector3 additionalVelocity;
    private float minStandingSpeed =1f;
    private bool isGrounded;                    // Is the player grounded?
    private float verticalVelocity = 0f;        // Vertical velocity to manage gravity forces
    private Vector3 slopeNormal;                // Store the slope normal for slope-related calculations

    public float uphillDirectionFactor = 0.5f;  // Factor to reduce movement in the uphill direction
    public float verticalToHorizontalFactor = 0.5f;  // Factor for vertical to horizontal velocity conversion

     [Header("Rotation Settings")]
    public Transform ballMesh;                  // Reference to the child with MeshRenderer
    public float maxRotationSpeed = 100f;       // Maximum rotation speed based on velocity

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  // Disable Unity's built-in gravity, we'll handle our own
        rb.constraints = RigidbodyConstraints.FreezeRotation;  // Prevent rotation, so movement is smooth

        // Grab the child GameObject that has a MeshRenderer component
        if (ballMesh == null)
        {
            ballMesh = GetComponentInChildren<MeshRenderer>().transform;
        }
    }

    void Update()
    {
        // Perform ground check every frame to see if the player is grounded
        CheckGroundStatus();

        // Optional: Log the current slope angle for debugging
        //if (isGrounded)
        //{
        //   float slopeAngle = GetSlopeAngle();
        //    Debug.Log($"Current Slope Angle: {slopeAngle}");

        //    Vector3 downhillDirection = GetDownhillDirection();
        //    Debug.Log($"Downhill Direction: {downhillDirection}");
        //}
    }

        // LateUpdate to draw velocity line and display the current velocity text
    void LateUpdate()
    {
        // Draw a line showing the velocity
        Vector3 velocityDirection = rb.velocity.normalized * 1.5f;  // Scale velocity to 1.5 units for the line
        Debug.DrawLine(transform.position, transform.position + velocityDirection, Color.red);

        
    }

    void FixedUpdate()
    {
        // Apply the player's movement based on input
        MovePlayer();

        // Apply custom gravity regardless of being grounded or not
        ApplyCustomGravity();
        
        // Apply final velocity to the rigidbody
        rb.velocity = movementVelocity + additionalVelocity;

        // Rotate the ball based on the velocity
        RotateBall();
    }

    // Input callback from the Unity Input System
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();  // Get movement input as a Vector2
    }

    // Rotate the ball mesh in the direction of the velocity
    void RotateBall()
    {
        if (ballMesh != null && (movementVelocity + additionalVelocity).magnitude > 0.1f)
        {
            // Get the velocity in the XZ plane (ignore Y)
            Vector3 velocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (velocityXZ.magnitude > 0.1f)
            {
                // Calculate the rotation axis based on the velocity (rotate around the axis perpendicular to movement)
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, velocityXZ).normalized;

                // Calculate the rotation speed based on the velocity magnitude
                float rotationSpeed = Mathf.Lerp(0f, maxRotationSpeed, velocityXZ.magnitude / (maxMovementSpeed + maxAdditionalSpeed));

                // Rotate the ball mesh around the calculated axis
                ballMesh.Rotate(rotationAxis, rotationSpeed * Time.fixedDeltaTime, Space.World);
            }
        }
    }

    void MovePlayer()
    {
        if (movementInput.magnitude > 0)
        {
            // Convert 2D input into 3D direction for XZ movement
            Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

            // Adjust the input direction based on the slope
            Vector3 moveDirectionOnSlope = AdjustDirectionToSlope(moveDirection);

            // If the player is on a slope, calculate the uphill factor
            if (IsOnSlope)
            {
                // Get the uphill direction by negating the downhill direction
                Vector3 uphillDirection = -GetDownhillDirection();

                // Calculate how much of the move direction is aligned with the uphill direction
                float uphillDot = Vector3.Dot(moveDirectionOnSlope.normalized, uphillDirection.normalized);

                // If the player is moving uphill, reduce the acceleration by the uphillDirectionFactor
                if (uphillDot > 0)  // Player is moving uphill
                {
                    // Reduce the acceleration based on how aligned the movement is with the uphill direction
                    moveDirectionOnSlope = uphillDirection * uphillDirectionFactor * uphillDot;
                    
                    Debug.Log($"Moving Uphill: Reducing movement by {uphillDot * uphillDirectionFactor}");
                }
            }

            // Calculate the target velocity based on the movement direction
            Vector3 targetVelocity = moveDirectionOnSlope * maxMovementSpeed;

            // Smoothly transition to the target velocity using acceleration
            movementVelocity = Vector3.MoveTowards(movementVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate when no input is provided
            if (additionalVelocity.magnitude > 0f)
            {
                additionalVelocity = Vector3.MoveTowards(additionalVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
                if (additionalVelocity.magnitude < 1f)
                {
                    movementVelocity += additionalVelocity;
                    additionalVelocity = Vector3.zero;
                }
            }
            else
            {
                movementVelocity = Vector3.MoveTowards(movementVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            }
        }
    }

    void ConvertVerticalToHorizontal()
    {
        if (!IsOnSlope && isGrounded)
        {
            // Calculate the horizontal direction (XZ plane) towards downhill
            Vector3 downhillDirection = GetDownhillDirection();

            // Convert part of the vertical velocity to horizontal velocity (only XZ plane)
            Vector3 verticalComponent = new Vector3(0, additionalVelocity.y, 0);
            Vector3 convertedVelocity = downhillDirection * verticalComponent.magnitude * verticalToHorizontalFactor;

            // Apply the converted velocity to the horizontal (XZ) plane
            movementVelocity += new Vector3(convertedVelocity.x, 0, convertedVelocity.z);

            // Reset vertical velocity after conversion
            additionalVelocity.y = 0;

            Debug.Log($"Converted {verticalComponent.magnitude * verticalToHorizontalFactor} of vertical velocity to horizontal");
        }
    }

    void ApplyCustomGravity()
    {
        if (isGrounded)
        {
            // If on slope, apply gravity along the slope
            if (IsOnSlope)
            {
                // Get the downhill direction of the slope
                Vector3 downhillDirection = GetDownhillDirection();

                // Apply gravity force in the downhill direction
                Vector3 slopeGravity = downhillDirection * Mathf.Abs(customGravity);

                // Apply the slope gravity. Only apply to additional when movement speed reaches max. 
                if (movementVelocity.magnitude >= maxMovementSpeed)
                {
                    additionalVelocity += slopeGravity * Time.fixedDeltaTime;
                    // Cap the additional velocity to ensure it doesn't exceed maxAdditionalSpeed
                    additionalVelocity = Vector3.ClampMagnitude(additionalVelocity, maxAdditionalSpeed);
                }
                else
                {
                    movementVelocity += slopeGravity * Time.fixedDeltaTime;
                }
            }
            else
            {
                // Convert vertical velocity to horizontal when transitioning from slope to flat ground
                ConvertVerticalToHorizontal();

                // Apply minimal downward force equivalent to gravity but prevent downward velocity accumulation
                additionalVelocity.y = 0;
            }
        }
        else
        {
            // When not grounded, apply full gravity (continuously increase downward velocity)
            additionalVelocity.y += customGravity * Time.fixedDeltaTime;
        }
    }

    // Check if the player is grounded using a raycast at the groundCheck position
    void CheckGroundStatus()
    {
        // Perform a line cast to check if the player is touching the ground
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckLength, groundLayer);

        if (isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 1f, groundLayer))
            {
                slopeNormal = hit.normal;  // Store the slope normal for slope adjustments
            }

            // Reset any downward velocity when grounded
            verticalVelocity = Mathf.Max(verticalVelocity, 0f);
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

    public float downhillRotationAngle = 20f;  // Angle to rotate downhill direction downward

    public Vector3 GetDownhillDirection()
    {
        RaycastHit hit;

        // Perform a raycast downwards to check the ground normal
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundCheckLength, groundLayer))
        {
            // Get the surface normal
            Vector3 groundNormal = hit.normal;

            // Calculate the downhill direction by projecting the normal onto the XZ plane
            Vector3 downhillDirection = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.down), groundNormal);
            downhillDirection.Normalize();

            // Now apply the rotation to the downhill direction
            downhillDirection = Quaternion.AngleAxis(downhillRotationAngle, Vector3.Cross(downhillDirection, Vector3.up)) * downhillDirection;

            // Draw a debug line in green to visualize the downhill direction in game mode
            //Debug.DrawLine(hit.point, hit.point + downhillDirection * 1.5f, Color.green, 1f); // 1.5 units long

            return downhillDirection;  // Return the normalized, rotated downhill direction
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

    // Check if the player is currently on a slope
    public bool IsOnSlope
    {
        get{
            float slopeAngle = GetSlopeAngle();
            return slopeAngle > 0 && slopeAngle <= 45f;  // Assuming 45 degrees is the max slope the player can walk on
        }
    }

    // Debugging: Draw a line in the editor to visualize the ground check area
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckLength);
        }
    }
}
