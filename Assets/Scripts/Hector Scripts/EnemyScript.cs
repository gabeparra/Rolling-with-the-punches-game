using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} defeated!");
        Destroy(gameObject);
    }
}