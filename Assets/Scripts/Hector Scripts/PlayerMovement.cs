using UnityEngine;

public class TrainPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 14f;
    public float jumpForce = 12f;
    public float rotationSpeed = 15f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Dash")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 input;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
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
            dashDirection = isMoving ? moveDirection : transform.forward;
            dashCooldownTimer = dashCooldown;
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
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
        }
        else if (moveDirection.magnitude > 0)
        {
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        }
    }

    bool IsGrounded()
    {
        if (groundLayer != 0)
            return Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f);
    }
}
