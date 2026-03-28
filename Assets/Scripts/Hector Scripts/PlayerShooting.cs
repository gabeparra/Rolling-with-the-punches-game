using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int damage = 3;
    public Transform firePoint;
    public float bulletSpeed = 100f;
    public AudioClip Gunshot;
    public Rigidbody trainRigidbody;
    public InputActionAsset inputActions;

    private AudioSource audioSource;
    private InputAction attackAction;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        var playerMap = inputActions.FindActionMap("Player");
        attackAction = playerMap.FindAction("Attack");
        attackAction.Enable();
    }

    void OnDestroy()
    {
        attackAction?.Disable();
    }

    void Update()
    {
        if (attackAction.WasPressedThisFrame())
        {
            ShootProjectile();
            audioSource.PlayOneShot(Gunshot);
        }
    }

    void ShootProjectile()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // Train is stationary — no velocity offset needed
        rb.linearVelocity = firePoint.right * bulletSpeed;

        Destroy(bullet, 2f);
    }
}
