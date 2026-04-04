using UnityEngine;

public class TrainPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float jumpForce = 6f;
    public float rotationSpeed = 15f;       // How snappily the player turns (higher = snappier)
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

        // Smooth rotation toward movement direction using Slerp
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Drive walk animation -- only plays while moving
        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        // Jump -- E key
        if (Input.GetKeyDown(KeyCode.E) && IsGrounded())
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        if (moveDirection.magnitude > 0)
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
    }
}