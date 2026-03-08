using UnityEngine;

public class TrainPlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f; // Small distance for flat surfaces like a train roof
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 input;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 1. PREVENT THE SPINNING: Freeze all physics-based rotation
        // This stops the character from tumbling or spinning when hitting wall colliders
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                         RigidbodyConstraints.FreezeRotationY | 
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 2. GET SNAPPY INPUT: Using GetAxisRaw removes the "slide" feel
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        // Calculate direction based on World Axis (W = +Z, D = +X)
        moveDirection = new Vector3(input.x, 0, input.y).normalized;

        // 3. SNAP ROTATION: If moving, look that way instantly
        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }

        // 4. JUMPING
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 5. MOVEMENT: Direct position manipulation for the best control on moving platforms
        if (moveDirection.magnitude > 0)
        {
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        }
    }

    bool IsGrounded()
    {
        // Using a small SphereCast is often more reliable than a Raycast on moving platforms
        return Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
    }
}