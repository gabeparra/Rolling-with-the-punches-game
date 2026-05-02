using UnityEngine;

public class WesternEnvironmentSpawner : EnvironmentSpawner
{
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

    void Start()
    {
        roadHalfWidth = 10f;

        SpawnLayer(buttePrefabs, butteCount, backgroundMinZ, backgroundMaxZ, true);
        SpawnLayer(backgroundCardPrefabs, backgroundCardCount, backgroundMaxZ, backgroundMaxZ + 20f, false);
        SpawnLayer(cactusPrefabs, cactusCount, midMinZ, midMaxZ, true);
        SpawnLayer(deadTreePrefabs, deadTreeCount, midMinZ, midMaxZ, true);
        SpawnLayer(rockPrefabs, rockCount, midMinZ, midMaxZ, true);
        SpawnLayer(propPrefabs, propCount, nearMinZ, nearMaxZ, true);
        SpawnLayer(grassPrefabs, grassCount, nearMinZ, nearMaxZ, false);
    }
}
