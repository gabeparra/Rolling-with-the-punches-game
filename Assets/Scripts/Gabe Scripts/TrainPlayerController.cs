using UnityEngine;
using UnityEngine.InputSystem;

public class TrainPlayerController : MonoBehaviour // Changed from 'PlayerMovement' class name -- Hector 4/24/26
{
    [Header("Movement")]
    public float speed = 14f;
    public float jumpForce = 5.5f;
    public float rotationSpeed = 15f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public float maxFallSpeed = 8f;

    [Header("Dash")]
    public float dashSpeed = 14f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 1f;

    [Header("Movement Mode")]
    public int movementMode = 1; // 1 = original; 2 = world-locked WASD (buttes/engine); 3 = simple world-axis WASD (W=+Z, S=-Z, D=+X, A=-X), snap-rotate
    public Vector3 buttesDirection = Vector3.forward;
    public Vector3 engineDirection = Vector3.right;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 input;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    private int jumpsUsed = 0;
    private const int maxJumps = 1;

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
        if (movementMode == 1)
        {
            // ---- MODE 1: original (Horizontal/Vertical axes, face movement direction) ----
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            moveDirection = new Vector3(input.x, 0, input.y).normalized;
        }
        else if (movementMode == 2)
        {
            // ---- MODE 2: world-locked WASD (W=buttes, S=non-butte, D=engine, A=caboose) ----
            Vector3 aim = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) aim += buttesDirection;
            if (Input.GetKey(KeyCode.S)) aim -= buttesDirection;
            if (Input.GetKey(KeyCode.D)) aim += engineDirection;
            if (Input.GetKey(KeyCode.A)) aim -= engineDirection;
            moveDirection = aim.sqrMagnitude > 0.01f ? aim.normalized : Vector3.zero;
        }
        else
        {
            // ---- MODE 3: simple world-axis WASD (W=+Z, S=-Z, D=+X, A=-X), snap-rotate to face direction ----
            Vector3 aim = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) aim += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) aim -= Vector3.forward;
            if (Input.GetKey(KeyCode.D)) aim += Vector3.right;
            if (Input.GetKey(KeyCode.A)) aim -= Vector3.right;
            moveDirection = aim.sqrMagnitude > 0.01f ? aim.normalized : Vector3.zero;
        }

        bool isMoving = moveDirection != Vector3.zero;

        if (isMoving && !isDashing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            if (movementMode == 3)
                transform.rotation = targetRotation; // snap-rotate so aim matches movement
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        // Jump — E or A. One per ground-touch.
        // Velocity gate: if we just jumped, vel.y is large positive; if falling,
        // it's negative. Grounded only when y-velocity is near zero AND we're
        // physically near a ground collider.
        bool sphereGrounded = IsGrounded();
        bool yStationary = Mathf.Abs(rb.linearVelocity.y) < 0.5f;
        bool firmlyGrounded = sphereGrounded && yStationary;

        if (firmlyGrounded) jumpsUsed = 0;

        if ((Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("GameJump"))
            && firmlyGrounded
            && jumpsUsed < maxJumps)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsUsed = maxJumps;
        }

        // Dash -- Left Shift or B button on Xbox
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("GameDash");
        var sprintAction = InputSystem.actions != null ? InputSystem.actions.FindAction("Sprint") : null;
        if (sprintAction != null && sprintAction.WasPressedThisFrame()) dashPressed = true;

        if (dashPressed && !isDashing && dashCooldownTimer <= 0f)
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
        // Loose CheckSphere — may catch player's own collider or the train deck.
        // Air-jumping is prevented by the y-velocity gate in Update, not by this check.
        Vector3 checkPos = transform.position + Vector3.down * groundCheckDistance;
        float radius = 0.3f;
        if (groundLayer != 0)
            return Physics.CheckSphere(checkPos, radius, groundLayer);
        return Physics.CheckSphere(checkPos, radius);
    }
}
