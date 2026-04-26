using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    void OnCollisionEnter(Collision collision)
    {
        // Enemy bullets damage the player; player tracers do hitscan damage
        // elsewhere (PlayerShooting.cs) and don't go through this path.
        if (CompareTag("EnemyBullet"))
        {
            PlayerHealth ph = collision.collider.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
