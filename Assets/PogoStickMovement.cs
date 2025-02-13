using UnityEngine;
using UnityEngine.InputSystem;

public class PogoStickMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float chargeSpeed = 5f;
    [SerializeField] private float maxCharge = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float leanSpeed = 90f; // Degrees per second while holding A/D
    [SerializeField] private float leanReturnSpeed = 45f; // Degrees per second when releasing A/D
    [SerializeField] private float maxLeanAngle = 30f; // Max lean angle in degrees

    [Header("References")]
    [SerializeField] private Transform pivot; // Pivot at player's feet
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private float charge = 0f;
    private bool isCharging = false;
    private float currentLeanAngle = 0f; // Current lean angle (positive = right, negative = left)
    private Rigidbody2D rb;
    private PlayerController playerControls;
    private bool wasGrounded; // Track previous frame's grounded state

    // Track lean input states
    private bool isLeaningLeft = false;
    private bool isLeaningRight = false;

    public Sprite spriteUp;
    public Sprite spriteLeft;
    public Sprite spriteRight;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerControls = new PlayerController();
    }

    void OnEnable()
    {
        playerControls.Enable();
        playerControls.Player.LeanLeft.started += _ => isLeaningLeft = true;
        playerControls.Player.LeanLeft.canceled += _ => isLeaningLeft = false;
        playerControls.Player.LeanRight.started += _ => isLeaningRight = true;
        playerControls.Player.LeanRight.canceled += _ => isLeaningRight = false;
        playerControls.Player.Charge.started += OnChargeStart;
        playerControls.Player.Charge.canceled += OnChargeRelease;
    }

    void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.LeanLeft.started -= _ => isLeaningLeft = true;
        playerControls.Player.LeanLeft.canceled -= _ => isLeaningLeft = false;
        playerControls.Player.LeanRight.started -= _ => isLeaningRight = true;
        playerControls.Player.LeanRight.canceled -= _ => isLeaningRight = false;
        playerControls.Player.Charge.started -= OnChargeStart;
        playerControls.Player.Charge.canceled -= OnChargeRelease;
    }

    void Update()
    {
        HandleLeaning();
        HandleCharge();
        UpdatePivotPosition();
        ResetMomentumOnLanding();
    }

    // --- Input Handlers ---
    void OnChargeStart(InputAction.CallbackContext context)
    {
        if (IsGrounded()) isCharging = true;
    }

    void OnChargeRelease(InputAction.CallbackContext context)
    {
        if (isCharging)
        {
            isCharging = false;
            Jump();
        }
    }

    // --- Leaning Logic ---
    void HandleLeaning()
    {
        // Determine lean direction
        float leanInput = 0f;
        if (isLeaningLeft && isLeaningRight)
        {
            // Both pressed: stop leaning (freeze angle)
            return;
        }
        else if (isLeaningLeft)
        {
            sr.sprite = spriteLeft;
            leanInput = -1f;
        }
        else if (isLeaningRight)
        {
            sr.sprite = spriteRight;
            leanInput = 1f;
        }

        if (leanInput != 0)
        {
            // Accumulate lean angle
            currentLeanAngle += leanInput * leanSpeed * Time.deltaTime;
            currentLeanAngle = Mathf.Clamp(currentLeanAngle, -maxLeanAngle, maxLeanAngle);
        }
        else
        {
            // Return to upright only if neither key is pressed
            if (!isLeaningLeft && !isLeaningRight)
            {
                sr.sprite = spriteUp;
                currentLeanAngle = Mathf.MoveTowards(currentLeanAngle, 0f, leanReturnSpeed * Time.deltaTime);
            }
        }

        // Apply rotation to the pivot and player
        RotatePlayerAroundPivot();
    }

    void RotatePlayerAroundPivot()
    {
        // Calculate the offset between the player and the pivot
        Vector2 offset = transform.position - pivot.position;

        // Rotate the offset vector by the current lean angle
        Quaternion rotation = Quaternion.Euler(0, 0, -currentLeanAngle);
        //Vector2 rotatedOffset = rotation * offset;

        // Update the player's position and rotation
        //transform.position = pivot.position + (Vector3)rotatedOffset;
        transform.rotation = rotation;
    }

    // --- Charging & Jumping ---
    void HandleCharge()
    {
        if (isCharging)
            charge = Mathf.Min(charge + chargeSpeed * Time.deltaTime, maxCharge);
    }

    void Jump()
    {
        if (!IsGrounded()) return;

        // Calculate jump direction based on lean angle
        float leanDirection = currentLeanAngle / maxLeanAngle; // Normalize to [-1, 1]
        Vector2 jumpDirection = (Vector2)(pivot.up + pivot.right * leanDirection).normalized;

        rb.AddForce(jumpDirection * charge * jumpForce, ForceMode2D.Impulse);
        charge = 0f;
    }

    // --- Ground Handling ---
    void ResetMomentumOnLanding()
    {
        bool isGrounded = IsGrounded();

        // Reset velocity when landing
        if (isGrounded && !wasGrounded)
            rb.linearVelocity = Vector2.zero;

        wasGrounded = isGrounded;
    }

    void UpdatePivotPosition()
    {
        // Keep pivot at player's feet
        Vector2 feetPosition = (Vector2)transform.position;
        pivot.position = feetPosition;
        groundCheck.position = feetPosition;
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
