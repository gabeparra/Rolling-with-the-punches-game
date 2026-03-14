using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int damage = 3; // Set weapon dmg here
    public Transform firePoint;
    public float bulletSpeed = 100f;
    public AudioClip Gunshot;
    private AudioSource audioSource;

    // Drag your Train object here in the Inspector
    public Rigidbody trainRigidbody; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootProjectile();
            audioSource.PlayOneShot(Gunshot);
        }
    }

    void ShootProjectile()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // We calculate the 'Muzzle Velocity' + 'Train Velocity'
        // This ensures the bullet doesn't 'drift' relative to the train
        Vector3 trainVelocity = (trainRigidbody != null) ? trainRigidbody.linearVelocity : Vector3.zero;
        
        rb.linearVelocity = (firePoint.right * bulletSpeed) + trainVelocity;

        Destroy(bullet, 2f);
    }
}