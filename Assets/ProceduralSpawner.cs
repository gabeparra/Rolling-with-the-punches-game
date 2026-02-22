using UnityEngine;

public class ProceduralSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject rockPrefab;
    public int numberOfRocks = 20;
    public float spawnRadius = 20f;
    
    void Start()
    {
        SpawnRocks();
    }
    
    void SpawnRocks()
    {
        for (int i = 0; i < numberOfRocks; i++)
        {
            // Random position in circle
            Vector3 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = new Vector3(randomPos.x, 0.5f, randomPos.y);
            
            // Spawn rock
            Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
