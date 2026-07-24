using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject tracerPrefab;
    public int damage = 1;
    public Transform firePoint;
    public float tracerSpeed = 30f;
    public float maxRange = 50f;
    public AudioClip gunshotClip;
    public AudioClip reloadClip;
    private AudioSource audioSource;
    private bool _reloading = false;
    private bool _triggerWasDown = false;
    public float autoFireInterval = 0.25f; // seconds between shots while the touch aim stick is held
    private float _nextAutoFire = 0f;

    void Update()
    {
        bool fps = ViewModeManager.FpsActive;

        // Shoot -- Left mouse click, RB, or Right Trigger on Xbox
        bool triggerDown = Input.GetAxis("GameShootTrigger") > 0.5f
            || (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.5f);
        bool triggerJustPressed = triggerDown && !_triggerWasDown;
        _triggerWasDown = triggerDown;

        // In FPS the trigger belongs exclusively to the auto-fire path below —
        // letting the edge-trigger branch also fire would double-shoot on the
        // first held frame.
        if ((Input.GetMouseButtonDown(0) || Input.GetButtonDown("GameShoot")
            || (triggerJustPressed && !fps)) && !_reloading)
        {
            if (HUDManager.Instance != null && !HUDManager.Instance.HasAmmo()) { Debug.LogWarning("[PlayerShooting] No ammo"); return; }
            FireRaycast();
        }

        // Auto-fire sources:
        //  - twin-stick modes on touch: dragging the aim zone both aims and
        //    fires (no spare finger for a trigger); same 0.04 deadzone as the
        //    aim-turn code in TrainPlayerController so aiming and firing
        //    engage together
        //  - first person: holding FIRE (mobile button or controller trigger)
        bool touchAim = !fps && MobileTouchControls.AimHeld
            && MobileTouchControls.AimVector.sqrMagnitude > 0.04f;
        bool padAim = !fps && MobileTouchControls.Active && Gamepad.current != null
            && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.04f;
        bool fpsHold = fps && triggerDown;
        if ((touchAim || padAim || fpsHold) && !_reloading && Time.time >= _nextAutoFire)
        {
            if (HUDManager.Instance == null || HUDManager.Instance.HasAmmo())
            {
                FireRaycast();
                _nextAutoFire = Time.time + autoFireInterval;
            }
        }
        // Reload -- automatic when the mag runs dry; R / X button / RELOAD
        // still work for early tactical reloads.
        bool magEmpty = HUDManager.Instance != null && !HUDManager.Instance.HasAmmo();
        if ((magEmpty || Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("GameReload")
            || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)) && !_reloading)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        _reloading = true;
        if (reloadClip != null && audioSource != null) audioSource.PlayOneShot(reloadClip);
        // Drive the RELOAD button's radial fill so phone players see progress.
        float t = 0f;
        while (t < PlayerStats.reloadTime)
        {
            MobileTouchControls.SetReloadProgress(t / PlayerStats.reloadTime);
            yield return null;
            t += Time.deltaTime;
        }
        MobileTouchControls.SetReloadProgress(0f);
        if (HUDManager.Instance != null) HUDManager.Instance.ReloadAmmo();
        _reloading = false;
    }

    void FireRaycast()
    {
        if (firePoint == null) { Debug.LogWarning("[PlayerShooting] firePoint is null"); return; }
        if (HUDManager.Instance != null) HUDManager.Instance.SpendAmmo();
        if (gunshotClip != null && audioSource != null) audioSource.PlayOneShot(gunshotClip);
        Debug.Log("[PlayerShooting] Firing!");

        Vector3 origin = firePoint.position;
        Vector3 direction = transform.forward;
        // First person aims with camera pitch — shoot along the camera ray.
        if (ViewModeManager.GetFpsAim(out Vector3 fpsOrigin, out Vector3 fpsDir))
        {
            origin = fpsOrigin;
            direction = fpsDir;
        }
        Vector3 hitPoint = origin + direction * maxRange;

        // Use RaycastAll to skip the player and train body — the player is
        // a child of the Train Engine whose MeshCollider would block every shot.
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Transform root = transform.root;
        foreach (RaycastHit hit in hits)
        {
            // Skip our own collider
            if (hit.collider.gameObject == gameObject) continue;

            // Check if this hit is on an enemy
            Bandit bandit = hit.collider.GetComponentInParent<Bandit>();
            ExplodeBarrel explodeBarrel = hit.collider.GetComponentInParent<ExplodeBarrel>();

            // Skip train parts (wheels, cabin, etc.) that aren't enemies —
            // the train has 27 child meshes with colliders that block bullets
            if (hit.collider.transform.IsChildOf(root) && bandit == null && explodeBarrel == null)
                continue;

            hitPoint = hit.point;
            if (bandit != null) bandit.TakeDamage(damage);
            if (explodeBarrel != null) explodeBarrel.TakeDamage(damage);
            break;
        }

        if (tracerPrefab != null)
        {
            GameObject tracer = Instantiate(tracerPrefab, origin, firePoint.rotation);
            tracer.layer = 8; // Same bullet layer — bullets ignore each other
            BulletTracer bt = tracer.GetComponent<BulletTracer>();
            if (bt == null) bt = tracer.AddComponent<BulletTracer>();
            Rigidbody rb = tracer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            // Disable collider — player tracers are visual-only (damage is hitscan).
            // Without this, the SphereCollider hits the train mesh instantly and destroys the tracer.
            Collider col = tracer.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            BulletVisuals.Enhance(tracer, new Color(1f, 0.95f, 0.3f), scale: 2.2f, trail: true);
            bt.Init(origin, hitPoint, tracerSpeed);
        }

        if (HUDManager.Instance != null && !HUDManager.Instance.HasAmmo() && !_reloading)
            StartCoroutine(Reload());
    }
}
