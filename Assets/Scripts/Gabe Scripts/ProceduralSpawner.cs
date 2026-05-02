using UnityEngine;

public class ProceduralSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject rockPrefab;
    public int numberOfRocks = 20;
    public float spawnRadius = 20f;

    [Header("Road Exclusion")]
    public float trackCenterZ = 10.2f;
    public float roadHalfWidth = 10f;

    void Start()
    {
        SpawnRocks();
    }

    void SpawnRocks()
    {
        if (rockPrefab == null) return;

        // BUG-06 fix: guard against infinite loop when road covers entire spawn area
        if (roadHalfWidth >= spawnRadius)
        {
            Debug.LogWarning("[ProceduralSpawner] roadHalfWidth >= spawnRadius — no valid spawn area. Skipping.");
            return;
        }

        for (int i = 0; i < numberOfRocks; i++)
        {
            Vector3 spawnPosition;
            int attempts = 0;
            const int maxAttempts = 50;

            do
            {
                // Place rocks along X (track direction) and offset Z around trackCenterZ
                // so rocks distribute evenly on both sides of the road
                float x = Random.Range(-spawnRadius, spawnRadius);
                float side = Random.value > 0.5f ? 1f : -1f;
                float z = trackCenterZ + side * Random.Range(roadHalfWidth, spawnRadius);
                spawnPosition = new Vector3(x, 0.5f, z);
                attempts++;
            }
            while (attempts < maxAttempts && IsOnRoad(spawnPosition));

            if (!IsOnRoad(spawnPosition))
                Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
        }
    }

    bool IsOnRoad(Vector3 pos)
    {
        // Tracks run along X at trackCenterZ, so check Z distance from that center
        return Mathf.Abs(pos.z - trackCenterZ) < roadHalfWidth;
    }
}
