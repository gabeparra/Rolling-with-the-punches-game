using UnityEngine;

public class TrainPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 14f;
    public float jumpForce = 12f;
    public float rotationSpeed = 15f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public float maxFallSpeed = 8f;

    [Header("Dash")]
    public float dashSpeed = 14f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 1f;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 input;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    /// <summary>0 = ready, 1 = just used. Used by DashCooldownBar.</summary>
    public float DashCooldownNormalized => Mathf.Clamp01(dashCooldownTimer / dashCooldown);

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        // Keep player at consistent world scale under the non-uniform train parent
        if (GetComponent<MaintainWorldScale>() == null)
        {
            var scaler = gameObject.AddComponent<MaintainWorldScale>();
            scaler.targetScale = 0.75f;
        }
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(input.x, 0, input.y).normalized;

        bool isMoving = moveDirection != Vector3.zero;

        if (isMoving && !isDashing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        // Jump -- E key or A button on Xbox
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("GameJump")) && IsGrounded())
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Dash -- Left Shift or B button on Xbox
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("GameDash")) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            // Flatten direction — never dash upward/downward
            Vector3 dir = isMoving ? moveDirection : transform.forward;
            dir.y = 0f;
            dashDirection = dir.normalized;
            dashCooldownTimer = dashCooldown;
            // Kill vertical velocity so dash doesn't launch player
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
            if (animator != null) animator.SetTrigger("Roll");
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // Dash horizontally only, zero vertical velocity to stay grounded
            rb.linearVelocity = new Vector3(
                dashDirection.x * dashSpeed,
                0f,
                dashDirection.z * dashSpeed);
        }
        else
        {
            // Set horizontal velocity from input, preserve vertical velocity
            Vector3 vel = rb.linearVelocity;
            vel.x = moveDirection.x * speed;
            vel.z = moveDirection.z * speed;
            rb.linearVelocity = vel;
        }

        // Clamp fall speed so player doesn't plummet through the train
        Vector3 v = rb.linearVelocity;
        if (v.y < -maxFallSpeed)
        {
            v.y = -maxFallSpeed;
            rb.linearVelocity = v;
        }
    }

    bool IsGrounded()
    {
        // CheckSphere detects overlapping colliders — works even when the
        // player has settled slightly into the surface (SphereCast misses that).
        Vector3 checkPos = transform.position + Vector3.down * groundCheckDistance;
        float radius = 0.3f;

        if (groundLayer != 0)
            return Physics.CheckSphere(checkPos, radius, groundLayer);
        return Physics.CheckSphere(checkPos, radius);
    }
}
