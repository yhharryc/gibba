using UnityEngine;
using TMPro;  // For TextMeshPro integration
using UnityEngine.InputSystem;  // For Input System integration

[RequireComponent(typeof(Rigidbody))]
public class TopDownBallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxMovementSpeed = 10f;        // Max speed for movement input
    public float maxAdditionalSpeed = 15f;      // Max speed for additional forces like gravity
    public float acceleration = 5f;             // How quickly the ball reaches max speed
    public float deceleration = 2f;             // How quickly the ball slows down when no input
    public float downhillAcceleration = 5f;     // Extra acceleration when moving downhill
    public float gravity = -9.81f;              // Custom gravity force
    public float uphillAccelerationReduction = 0.5f; // Percentage of reduction when moving uphill (50% reduction)

    [Header("UI References")]
    public TextMeshProUGUI movementVelocityText;    // Reference for movement velocity TextMeshPro
    public TextMeshProUGUI additionalVelocityText;  // Reference for additional velocity TextMeshPro

    [Header("Slope Detection")]
    public float slopeThreshold = 0.5f;         // Minimum slope angle to apply slope behavior
    public float slopeMaxAngle = 45f;           // Maximum slope angle for movement

    [Header("Physics")]
    public LayerMask groundLayer;               // Layer to detect the ground
    public Transform groundCheck;               // A point at the bottom of the ball to detect ground

    private Rigidbody rb;
    private Vector2 movementInput;              // Updated to Vector2 for input
    private Vector3 movementVelocity;           // Velocity from player movement
    private Vector3 additionalVelocity;         // Velocity from gravity and other forces
    private Vector3 slopeDirection;

    private bool isGrounded;
    private float slopeAngle;
    private float verticalVelocity = 0f;        // For custom gravity application

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  // Disable built-in gravity
        rb.constraints = RigidbodyConstraints.FreezeRotation;  // Prevent rotation, movement controls rolling
    }

    void Update()
    {
        CheckGroundAndSlope();
        UpdateVelocityUI();  // Update UI Text fields with velocity data
    }

    void FixedUpdate()
    {
        MoveBall();
        ApplyCustomGravity();
    }

    // Input System callback to handle player movement input (Vector2 input)
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();  // Get movement input as a Vector2
    }

    // Move the ball with applied physics and slope consideration
    void MoveBall()
    {
        if (movementInput.magnitude > 0)
        {
            // Convert the 2D movement input to 3D for movement on the XZ plane
            Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y);
            moveDirection = AdjustDirectionToSlope(moveDirection);

            // Calculate acceleration based on whether moving uphill or downhill
            float adjustedAcceleration = CalculateAdjustedAcceleration(moveDirection);
            Debug.Log($"Adjusted Acceleration: {adjustedAcceleration}");

            Vector3 targetVelocity = moveDirection * maxMovementSpeed;

            // Apply velocity changes manually for movement input
            movementVelocity += moveDirection * adjustedAcceleration * Time.fixedDeltaTime;

            // Clamp movement velocity to prevent exceeding maxMovementSpeed
            movementVelocity = Vector3.ClampMagnitude(movementVelocity, maxMovementSpeed);

            // Apply interaction between movement and additional velocity
            if (maxMovementSpeed - movementVelocity.magnitude < 0.3f)
            {
                HandleAdditionalVelocityInteraction(moveDirection);
            }
        }
        else
        {
            // Decelerate movement velocity when no input is given
            movementVelocity = Vector3.MoveTowards(movementVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        // Combine movement velocity and additional velocity from gravity or slope before applying it to the rigidbody
        Vector3 finalVelocity = movementVelocity + additionalVelocity;

        // Clamp final velocity to prevent exceeding maxAdditionalSpeed in the additional forces
        finalVelocity = Vector3.ClampMagnitude(finalVelocity, maxAdditionalSpeed+maxMovementSpeed);

        // Apply the combined velocity to the rigidbody
        rb.velocity = new Vector3(finalVelocity.x, verticalVelocity, finalVelocity.z);
    }

    void HandleAdditionalVelocityInteraction(Vector3 moveDirection)
    {
        // Get the dot product to determine whether moving with or against additional velocity
        float dot = Vector3.Dot(moveDirection.normalized, additionalVelocity.normalized);

        if (dot < 0)  // Moving against the direction of the additional velocity
        {
            // Reduce additional velocity based on the movement acceleration
            additionalVelocity += moveDirection.normalized * acceleration * Time.fixedDeltaTime;

            // Clamp to avoid over-compensating
            additionalVelocity = Vector3.ClampMagnitude(additionalVelocity, maxAdditionalSpeed);

            Debug.Log($"Moving Against Additional Velocity: Adjusting Downward. Additional Velocity: {additionalVelocity}");
        }
        else if (dot > 0)  // Moving in the same direction as additional velocity
        {
            // Moving with the additional velocity doesn't increase it
            // but you still move with the current additional velocity
            Debug.Log("Moving with Additional Velocity: No Effect on Additional Velocity");
        }
    }

    // Update the velocity information in the TextMeshPro components
    void UpdateVelocityUI()
    {
        // Check if the text objects are assigned
        if (movementVelocityText != null)
        {
            // Update the text with movement velocity information
            movementVelocityText.text = $"Movement Velocity: {movementVelocity.magnitude:F2}";
        }

        if (additionalVelocityText != null)
        {
            // Update the text with additional velocity information
            additionalVelocityText.text = $"Additional Velocity: {additionalVelocity.magnitude:F2}";
        }
    }

    // Calculate the adjusted acceleration based on whether moving uphill or downhill
    float CalculateAdjustedAcceleration(Vector3 moveDirection)
    {
        float adjustedAcceleration = acceleration;

        if (slopeAngle > slopeThreshold && slopeDirection != Vector3.zero)
        {
            // Check if moving downhill (dot product > 0) or uphill (dot product < 0)
            float dot = Vector3.Dot(moveDirection.normalized, slopeDirection.normalized);
            Debug.Log($"Dot Product (Move Direction vs. Slope): {dot}");

            if (dot > 0)  // Moving downhill
            {
                // Moving downhill, increase acceleration
                adjustedAcceleration += downhillAcceleration;  // Add both accelerations
                Debug.Log("Moving Downhill: Acceleration Increased");
            }
            else if (dot < 0)  // Moving uphill
            {
                // Moving uphill, reduce acceleration by a percentage
                float uphillReductionFactor = 1 - uphillAccelerationReduction;
                adjustedAcceleration *= uphillReductionFactor;  // Reduce acceleration uphill by a percentage
                Debug.Log($"Moving Uphill: Acceleration Reduced by {uphillReductionFactor * 100}%");
            }
        }

        return adjustedAcceleration;
    }

    // Adjusts the movement input direction according to the slope
    Vector3 AdjustDirectionToSlope(Vector3 inputDirection)
    {
        if (slopeAngle > 0 && slopeAngle <= slopeMaxAngle)
        {
            // Project movement direction onto the slope
            Vector3 adjustedDirection = Vector3.ProjectOnPlane(inputDirection, slopeDirection);
            Debug.Log($"Adjusted Movement Direction: {adjustedDirection}");
            return adjustedDirection.normalized;
        }

        return inputDirection;  // Flat ground or no significant slope
    }

    // Apply custom gravity, converting vertical velocity to downhill velocity when on a slope
    void ApplyCustomGravity()
    {
        if (isGrounded)
        {
            if (slopeAngle > slopeThreshold && slopeDirection != Vector3.zero)
            {
                // Apply projected gravity along the slope direction when grounded
                Vector3 projectedGravity = Vector3.ProjectOnPlane(Vector3.down * gravity, slopeDirection.normalized);
                additionalVelocity += projectedGravity * Time.fixedDeltaTime;  // Add downhill gravity

                Debug.Log($"Grounded on Slope: Projected Gravity: {projectedGravity}, Additional Velocity: {additionalVelocity}");
            }
            else if (slopeAngle <= slopeThreshold && additionalVelocity.magnitude > 0)
            {
                // Transition from slope to flat ground: Convert vertical momentum to horizontal
                Vector3 horizontalMomentum = new Vector3(additionalVelocity.x, 0, additionalVelocity.z);
                additionalVelocity = horizontalMomentum;  // Retain the horizontal part of the velocity

                Debug.Log($"Transitioned to Flat: Preserving Horizontal Momentum: {additionalVelocity}");
            }
            else
            {
                // Grounded on flat surface, reset vertical velocity but retain horizontal momentum
                verticalVelocity = 0f;
                // Keep additional velocity intact (already set in the transition condition)
                Debug.Log($"Flat Ground: No Additional Gravity Applied, Momentum Preserved: {additionalVelocity}");
            }
        }
        else
        {
            // Not grounded, apply normal vertical gravity (free-fall)
            verticalVelocity += gravity * Time.fixedDeltaTime;
            Debug.Log($"In Air: Vertical Gravity Applied: {verticalVelocity}");
        }
    }

    // Check whether the ball is grounded and calculate the slope angle
    void CheckGroundAndSlope()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 0.2f, groundLayer);

        if (isGrounded)
        {
            // Calculate slope direction and angle
            slopeDirection = Vector3.Cross(Vector3.Cross(hit.normal, Vector3.down), hit.normal);
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            Debug.Log($"Slope Angle: {slopeAngle}, Slope Direction: {slopeDirection}");
        }
        else
        {
            slopeDirection = Vector3.zero;
            slopeAngle = 0;
        }
    }

    // Debugging: Draw lines in Scene view to visualize movement and additional velocities
    [Header("Gizmos Settings")]
    public float minLineLength = 0f;  // Minimum line length for visualization
    public float maxLineLength = 5f;    // Maximum line length for visualization

    // Debugging: Draw lines in Scene view to visualize movement and additional velocities
    void OnDrawGizmos()
    {
        // Interpolate movement velocity to fit between the minimum and maximum line length
        float movementVelocityMagnitude = Mathf.Clamp(movementVelocity.magnitude, 0, maxMovementSpeed);
        float movementLineLength = Mathf.Lerp(minLineLength, maxLineLength, movementVelocityMagnitude / maxMovementSpeed);
        Vector3 movementLine = movementVelocity.normalized * movementLineLength;

        // Interpolate additional velocity to fit between the minimum and maximum line length
        float additionalVelocityMagnitude = Mathf.Clamp(additionalVelocity.magnitude, 0, maxAdditionalSpeed);
        float additionalLineLength = Mathf.Lerp(minLineLength, maxLineLength, additionalVelocityMagnitude / maxAdditionalSpeed);
        Vector3 additionalLine = additionalVelocity.normalized * additionalLineLength;

        // Draw movement velocity line (green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + movementLine);

        // Draw additional velocity line (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + additionalLine);
    }

}
