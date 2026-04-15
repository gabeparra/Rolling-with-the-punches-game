using UnityEngine;
/*
    This script attaches to bullet prefab on its spawning. 
*/
public class BulletTracer : MonoBehaviour
{
    private Vector3 _target; // Defines target that the bullet will collide with 
    private float _speed; // Speed velocity of the visual bullet
    private float _timeout = 3f; // This will ensure bullet with auto despawn after timeout period 
    private bool _visualOnly; // Reminant feature from when I had the physical bullet -- easily reversible

    public void Init(Vector3 origin, Vector3 hitPoint, float tracerSpeed) // Initializes the bullet on spawn
    {
        transform.position = origin; // Sets bullet to origin of its firepoint
        _target = hitPoint; // Currently set to target hit point value
        _speed = tracerSpeed; // Speed of the tracer itself that targets 
        _visualOnly = true; // Boolean for collision vs visual only effect
        transform.LookAt(_target); // Bullet object rotates to face its target to set inital trajectory
    }

    void Update()
    {
        _timeout -= Time.deltaTime; // This is the time out variable to ensure despawning after a set time without collision 
        if (_timeout <= 0f) { Destroy(gameObject); return; } // Here we destory the object once the object times out 

        if (_visualOnly)
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime); // Moves with the raycast
            if (Vector3.Distance(transform.position, _target) < 0.05f) // Destroy the visual if the object gets close to the target
                Destroy(gameObject); // Despawns the game object 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (CompareTag("EnemyBullet")) // Checks if the bullet is coming from an enemy firepoint
        {
            PlayerHealth ph = collision.collider.GetComponentInParent<PlayerHealth>(); // If so, on collision do an action
            if (ph != null) ph.TakeDamage(1); // The action here is reduce the player health by 1
        }
        Destroy(gameObject); // Destroys the bullet after the collision
    }
}