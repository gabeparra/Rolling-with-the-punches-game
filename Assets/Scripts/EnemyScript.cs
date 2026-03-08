using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public int goldReward = 50;
    
    private Renderer enemyRenderer;
    private Color originalColor;
    private bool isDead = false;

    void Start()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();
        originalColor = enemyRenderer.material.color;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        StartCoroutine(FlashRed(0.1f));

        if (health <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator FlashRed(float duration)
    {
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(duration);
        enemyRenderer.material.color = originalColor;
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        
        // Flash twice
        for (int i = 0; i < 2; i++)
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            enemyRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        // Give Gold to Player
        FindObjectOfType<GoldManager>().AddGold(goldReward);
        
        Destroy(gameObject);
    }
}