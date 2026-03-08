using UnityEngine;

public class ProceduralSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject rockPrefab;
    public int numberOfRocks = 20;
    public float spawnRadius = 20f;

    [Header("Road Exclusion")]
    public float roadHalfWidth = 3f;

    void Start()
    {
        SpawnRocks();
    }

    void SpawnRocks()
    {
        for (int i = 0; i < numberOfRocks; i++)
        {
            Vector3 spawnPosition;

            // Keep generating positions until one is outside the road
            do
            {
                Vector3 randomPos = Random.insideUnitCircle * spawnRadius;
                spawnPosition = new Vector3(randomPos.x, 0.5f, randomPos.y);
            }
            while (IsOnRoad(spawnPosition));

            Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
        }
    }

    bool IsOnRoad(Vector3 pos)
    {
        // Tracks run along X, so check Z distance from center
        return Mathf.Abs(pos.z) < roadHalfWidth;
    }
}
