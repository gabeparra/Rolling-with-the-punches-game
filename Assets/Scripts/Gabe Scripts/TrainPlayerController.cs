using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//<<<<<<< HEAD
public class TrainPlayerController : MonoBehaviour // Changed from 'PlayerMovement' class name -- Hector 4/24/26
//=======
//public class PlayerMovement : MonoBehaviour
//>>>>>>> 5ea8deb (Adding comments to my scripts. Still have more to comment. Added comments to the following scripts: BulletTracer.cs, CusorImage.cs, EnemyScript.cs, and partially through HudManager script. I also deleted several scripts that were redundant or no longer in use such as CanvasDebugger and GoldManager scripts. Several classes also renamed to reflect their proper class names.)
{
    [Header("Needed for Hub")]
    [Tooltip("Minor changes necessary while in the hub")]
    private bool hubMode = false;
    public static bool canMove = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => canMove = true;

    [Header("Movement")]
    public float jumpForce = 5.5f;
    public float rotationSpeed = 30f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public float maxFallSpeed = 8f;

    [Header("Movement Feel")]
    [Tooltip("Units/sec² when ramping up to top speed.")]
    public float acceleration = 80f;
    [Tooltip("Units/sec² when no input is held — higher = snappier stops.")]
    public float deceleration = 120f;

    [Header("Jump Feel")]
    [Tooltip("Grace period after walking off a ledge during which jump still works.")]
    public float coyoteTime = 0.1f;
    [Tooltip("If jump is pressed within this window before landing, it fires on touchdown.")]
    public float jumpBufferTime = 0.15f;
    [Tooltip("Extra gravity multiplier while falling — 1.8 kills the floaty apex feeling.")]
    public float fallGravityMultiplier = 1.8f;

    [Header("Dash")]
    public float dashSpeed = 22f;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 input;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    private int jumpsUsed = 0;

    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private int groundedFrames = 0;
    private const int requiredGroundedFrames = 1; // groundLayer is set, so IsGrounded is reliable — no buffer needed

    /// <summary>0 = ready, 1 = just used. Used by DashCooldownBar.</summary>
    public float DashCooldownNormalized => Mathf.Clamp01(dashCooldownTimer / PlayerStats.dashCooldown);

    /// <summary>True while a dash is active — used by PlayerHealth for damage immunity.</summary>
    public bool IsDashing => isDashing;

    /// <summary>
    /// Refunds part of the dash cooldown — currently called by PlayerHealth
    /// on a "perfect dodge" (an enemy bullet that would have hit during a
    /// dash). Encourages dashing INTO bullets rather than away from them.
    /// </summary>
    public void RefundDashCooldown(float fraction)
    {
        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - PlayerStats.dashCooldown * fraction);
    }

    void Start()
    {
        hubMode = SceneManager.GetActiveScene().name == "HubScene";
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        // Keep player at consistent world scale under the non-uniform train parent
        if (!hubMode && GetComponent<MaintainWorldScale>() == null)
        {
            var scaler = gameObject.AddComponent<MaintainWorldScale>();
            scaler.targetScale = 0.75f;
            Debug.Log("scaler created");
        }

        // Visual + cooldown sweeteners for the dash (trail, FOV punch, ring
        // pulse, perfect-dodge slow-mo). Self-contained component.
        if (GetComponent<DashEffects>() == null)
        {
            gameObject.AddComponent<DashEffects>();
        }
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        // Legacy axes can't see Input System devices (touch joystick emulates
        // a virtual gamepad), so merge the left stick in manually.
        if (input.sqrMagnitude < 0.01f && Gamepad.current != null)
            input = Gamepad.current.leftStick.ReadValue();
        // Vector input from controller can have magnitude < 1 (analog stick).
        // Clamp magnitude so diagonal isn't faster than cardinal.
        Vector3 raw = new Vector3(input.x, 0, input.y);
        // First person: stick-up means "where I'm looking", not world +Z.
        if (ViewModeManager.FpsActive)
            raw = Quaternion.Euler(0f, ViewModeManager.Yaw, 0f) * raw;
        if(canMove)
        {
            moveDirection = raw.sqrMagnitude > 1f ? raw.normalized : raw;
        }
        else moveDirection = Vector3.zero;

        bool isMoving = moveDirection.sqrMagnitude > 0.01f;

        // Aim by mouse — independent of movement direction. Cast ray from camera
        // through cursor onto a horizontal plane at the player's height, then
        // rotate the player to face the hit point. Falls back to controller right
        // stick (Look action) if no mouse delta and the right stick is pushed.
        // First person: ViewModeManager's look drives the body rotation.
        if (!isDashing && !ViewModeManager.FpsActive)
        {
            Vector3 aimDir = transform.forward;
            Camera cam = Camera.main;
            // On phones there is no real cursor — a stale mousePosition would
            // yank the aim toward a screen corner, so mouse aim is skipped and
            // the touch aim stick (right stick below) is the only aim source.
            if (cam != null && !MobileTouchControls.Active)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Plane aimPlane = new Plane(Vector3.up, transform.position);
                if (aimPlane.Raycast(ray, out float dist))
                {
                    Vector3 hitPoint = ray.GetPoint(dist);
                    Vector3 toMouse = hitPoint - transform.position;
                    toMouse.y = 0f;
                    if (toMouse.sqrMagnitude > 0.01f)
                        aimDir = toMouse.normalized;
                }
            }

            // Stick override: the floating touch aim zone while held, else a
            // real controller's right stick (read at device level — the Look
            // action is also bound to <Pointer>/delta, which touch drags feed).
            // Reads camera-relative so pushing up aims away from the camera.
            Vector2 stick = MobileTouchControls.AimHeld
                ? MobileTouchControls.AimVector
                : (Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero);
            if (stick.sqrMagnitude > 0.04f)
            {
                Vector3 stickWorld = new Vector3(stick.x, 0f, stick.y);
                if (cam != null)
                {
                    // With a steep top-down camera, forward projects to
                    // ~zero on the ground plane — use camera up instead so
                    // the math never degenerates to a zero vector.
                    Vector3 camFwd = cam.transform.forward; camFwd.y = 0f;
                    if (camFwd.sqrMagnitude < 1e-4f) { camFwd = cam.transform.up; camFwd.y = 0f; }
                    camFwd.Normalize();
                    Vector3 camRight = cam.transform.right; camRight.y = 0f; camRight.Normalize();
                    stickWorld = camRight * stick.x + camFwd * stick.y;
                }
                if (stickWorld.sqrMagnitude > 1e-4f)
                    aimDir = stickWorld.normalized;
            }
            else if (MobileTouchControls.Active && isMoving)
            {
                // Touch with no aim engaged (e.g. walking the hub, or running
                // between fights): face where you're going, twin-stick style.
                // Replaces the pre-zone behavior where the old hub aim stick
                // was the only way to turn the character on phones.
                aimDir = moveDirection.normalized;
            }

            Quaternion targetRotation = Quaternion.LookRotation(aimDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        // --- Ground & coyote-time tracking ---
        bool sphereGrounded = IsGrounded();
        bool yStationary = Mathf.Abs(rb.linearVelocity.y) < 0.5f;
        bool firmlyGrounded = sphereGrounded && yStationary;

        // A 1-frame `firmlyGrounded` flicker at jump apex (where vel.y crosses 0
        // and the loose sphere check may return true if groundLayer isn't set)
        // would otherwise refresh coyote/jumpsUsed and allow a free second jump.
        // Require several consecutive grounded frames to count as a real landing.
        if (firmlyGrounded) groundedFrames++;
        else groundedFrames = 0;

        if (groundedFrames >= requiredGroundedFrames)
        {
            jumpsUsed = 0;
            coyoteTimer = coyoteTime; // refresh grace window
        }
        else if (!firmlyGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }

        // --- Jump input buffering ---
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("GameJump")
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        if (jumpPressed) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        // Jump fires if buffer is active AND we're within coyote window AND have a jump available.
        if (jumpBufferTimer > 0f && coyoteTimer > 0f && jumpsUsed < PlayerStats.maxJumps && canMove)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsUsed++;
            jumpBufferTimer = 0f; // consume buffer
            coyoteTimer = 0f;     // consume coyote
        }

        // --- Dash --- (E on keyboard, B button on Xbox)
        // Was Shift; rebound to E because Shift was triggering Windows
        // sticky-keys during play. Sprint action listener removed for the
        // same reason — its action-map binding is still Shift.
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        bool dashPressed = Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("GameDash")
            || (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);

        if (dashPressed && !isDashing && dashCooldownTimer <= 0f && canMove)
        {
            isDashing = true;
            dashTimer = PlayerStats.dashTime;
            Vector3 dir = isMoving ? moveDirection.normalized : transform.forward;
            dir.y = 0f;
            dashDirection = dir.normalized;
            dashCooldownTimer = PlayerStats.dashCooldown;
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
        if (isDashing && canMove)
        {
            // Dash overrides movement entirely.
            rb.linearVelocity = new Vector3(
                dashDirection.x * dashSpeed,
                0f,
                dashDirection.z * dashSpeed);
        }
        else
        {
            // Smooth horizontal velocity toward the target. Acceleration when
            // input is held, deceleration when released — different rates so
            // stops feel snappier than starts.
            Vector3 currentVel = rb.linearVelocity;
            Vector3 targetVel = new Vector3(moveDirection.x * PlayerStats.moveSpeed, currentVel.y, moveDirection.z * PlayerStats.moveSpeed);

            bool inputHeld = moveDirection.sqrMagnitude > 0.01f;
            float rate = inputHeld ? acceleration : deceleration;

            // Only smooth horizontal axes; preserve gravity-driven y.
            Vector3 horizCurrent = new Vector3(currentVel.x, 0f, currentVel.z);
            Vector3 horizTarget = new Vector3(targetVel.x, 0f, targetVel.z);
            Vector3 horizNew = Vector3.MoveTowards(horizCurrent, horizTarget, rate * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(horizNew.x, currentVel.y, horizNew.z);
        }

        // Extra fall gravity — kills the floaty apex.
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // Clamp fall speed so player doesn't plummet through the train.
        Vector3 v = rb.linearVelocity;
        if (v.y < -maxFallSpeed)
        {
            v.y = -maxFallSpeed;
            rb.linearVelocity = v;
        }
        //Debug.Log($"Current velocity:{rb.linearVelocity.magnitude}");
    }

    bool IsGrounded()
    {
        // Cast a sphere downward from slightly above the player's feet so it
        // doesn't start inside the player's capsule (which would skip the
        // first overlap on some Unity versions). Distance covers ~1m below feet.
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        const float radius = 0.3f;
        const float distance = 1.0f;

        // Strict: groundLayer if set. Hits ONLY ground-tagged colliders.
        if (groundLayer != 0 && Physics.SphereCast(origin, radius, Vector3.down, out _, distance, groundLayer, QueryTriggerInteraction.Ignore))
            return true;

        // Fallback: any layer, but skip self/children.
        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, Vector3.down, distance, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            Transform t = hits[i].transform;
            if (t == transform || t.IsChildOf(transform)) continue;
            return true;
        }
        return false;
    }
}
