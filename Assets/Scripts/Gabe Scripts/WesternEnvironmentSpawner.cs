using UnityEngine;

public class WesternEnvironmentSpawner : MonoBehaviour
{
    [Header("Track Settings")]
    public float trackLength = 200f;
    public float trackCenterZ = 0f;

    [Header("Background (far from track)")]
    public GameObject[] buttePrefabs;
    public GameObject[] backgroundCardPrefabs;
    public int butteCount = 8;
    public int backgroundCardCount = 4;
    public float backgroundMinZ = 40f;
    public float backgroundMaxZ = 80f;

    [Header("Mid Range")]
    public GameObject[] cactusPrefabs;
    public GameObject[] deadTreePrefabs;
    public GameObject[] rockPrefabs;
    public int cactusCount = 30;
    public int deadTreeCount = 15;
    public int rockCount = 20;
    public float midMinZ = 10f;
    public float midMaxZ = 40f;

    [Header("Near Track (decoration)")]
    public GameObject[] propPrefabs;
    public GameObject[] grassPrefabs;
    public int propCount = 15;
    public int grassCount = 40;
    public float nearMinZ = 3f;
    public float nearMaxZ = 10f;

    [Header("Road Exclusion")]
    public float roadHalfWidth = 5f;

    [Header("Scale Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.3f;

    void Start()
    {
        SpawnLayer(buttePrefabs, butteCount, backgroundMinZ, backgroundMaxZ, true);
        SpawnLayer(backgroundCardPrefabs, backgroundCardCount, backgroundMaxZ, backgroundMaxZ + 20f, false);
        SpawnLayer(cactusPrefabs, cactusCount, midMinZ, midMaxZ, true);
        SpawnLayer(deadTreePrefabs, deadTreeCount, midMinZ, midMaxZ, true);
        SpawnLayer(rockPrefabs, rockCount, midMinZ, midMaxZ, true);
        SpawnLayer(propPrefabs, propCount, nearMinZ, nearMaxZ, true);
        SpawnLayer(grassPrefabs, grassCount, nearMinZ, nearMaxZ, false);
    }

    void SpawnLayer(GameObject[] prefabs, int count, float minZ, float maxZ, bool randomRotation)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-trackLength / 2f, trackLength / 2f);

            // Spawn on both sides of the track, never inside the road
            float side = Random.value > 0.5f ? 1f : -1f;
            float effectiveMinZ = Mathf.Max(minZ, roadHalfWidth);
            float z = trackCenterZ + side * Random.Range(effectiveMinZ, maxZ);

            Vector3 pos = new Vector3(x, 0f, z);
            Quaternion rot = randomRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : Quaternion.identity;

            GameObject obj = Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, rot, transform);

            float scale = Random.Range(minScale, maxScale);
            obj.transform.localScale *= scale;
        }
    }
}
