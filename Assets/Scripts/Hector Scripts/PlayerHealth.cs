using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public void TakeDamage(int amount)
    {
        for (int i = 0; i < amount; i++)
            if (HUDManager.Instance != null)
                HUDManager.Instance.LoseHeart();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}