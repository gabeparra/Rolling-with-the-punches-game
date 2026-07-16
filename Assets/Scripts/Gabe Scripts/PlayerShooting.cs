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
        // Shoot -- Left mouse click, RB, or Right Trigger on Xbox
        bool triggerDown = Input.GetAxis("GameShootTrigger") > 0.5f
            || (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.5f);
        bool triggerJustPressed = triggerDown && !_triggerWasDown;
        _triggerWasDown = triggerDown;

        if ((Input.GetMouseButtonDown(0) || Input.GetButtonDown("GameShoot") || triggerJustPressed) && !_reloading)
        {
            if (HUDManager.Instance != null && !HUDManager.Instance.HasAmmo()) { Debug.LogWarning("[PlayerShooting] No ammo"); return; }
            FireRaycast();
        }

        // Touch: deflecting the aim stick both aims and auto-fires (twin-stick
        // style) — there's no spare finger for a trigger. Same deadzone as the
        // aim code so shots only start once aiming is engaged. Mobile-only so
        // desktop controller players don't fire just by aiming.
        bool stickHeld = MobileTouchControls.Active && Gamepad.current != null
            && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.25f;
        if (stickHeld && !_reloading && Time.time >= _nextAutoFire)
        {
            if (HUDManager.Instance == null || HUDManager.Instance.HasAmmo())
            {
                FireRaycast();
                _nextAutoFire = Time.time + autoFireInterval;
            }
        }
        // Reload -- R or X button on Xbox
        if ((Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("GameReload")
            || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)) && !_reloading)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        _reloading = true;
        if (reloadClip != null && audioSource != null) audioSource.PlayOneShot(reloadClip);
        yield return new WaitForSeconds(PlayerStats.reloadTime);
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
