using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int health = 5;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            if (HUDManager.Instance != null)
                HUDManager.Instance.OnEnemyKilled();
            Destroy(gameObject);
        }
    }
}
