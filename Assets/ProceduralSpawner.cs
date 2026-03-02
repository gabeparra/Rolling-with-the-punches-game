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
            // Posición random en círculo
            Vector3 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = new Vector3(randomPos.x, 0.5f, randomPos.y);
            
            // Spawna la roca
            Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
