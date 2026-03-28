using UnityEngine;
using UnityEngine.InputSystem;

public class TrainPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public InputActionAsset inputActions;

    [Header("Dash")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        var playerMap = inputActions.FindActionMap("Player");
        playerMap.Enable();
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        dashAction = playerMap.FindAction("Sprint");
    }

    void OnDestroy()
    {
        var playerMap = inputActions?.FindActionMap("Player");
        playerMap?.Disable();
    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }

        if (jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Dash
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (dashAction.WasPressedThisFrame() && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            // Dash in move direction, or forward if standing still
            dashDirection = moveDirection != Vector3.zero ? moveDirection : transform.forward;
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
        // Raycast downward from slightly above the player's feet
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f, groundLayer)
            || (groundLayer == 0 && Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f));
    }
}
