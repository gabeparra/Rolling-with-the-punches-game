using UnityEngine;

public class SnowyEnvironmentSpawner : EnvironmentSpawner
{
    [Header("Background (far from track)")]
    public GameObject[] mountainPrefabs;
    public GameObject[] snowDriftPrefabs;
    public int mountainCount = 8;
    public int snowDriftCount = 6;
    public float backgroundMinZ = 40f;
    public float backgroundMaxZ = 80f;

    [Header("Mid Range")]
    public GameObject[] pineTreePrefabs;
    public GameObject[] snowRockPrefabs;
    public GameObject[] logPrefabs;
    public int pineTreeCount = 25;
    public int snowRockCount = 20;
    public int logCount = 10;
    public float midMinZ = 10f;
    public float midMaxZ = 40f;

    [Header("Near Track (decoration)")]
    public GameObject[] propPrefabs;
    public GameObject[] snowPilePrefabs;
    public int propCount = 15;
    public int snowPileCount = 30;
    public float nearMinZ = 3f;
    public float nearMaxZ = 10f;

    [Header("Snow Tint")]
    public bool tintObjects = true;
    public Color snowTint = new Color(0.85f, 0.88f, 0.95f);
    public float tintStrength = 0.55f;

    void Awake()
    {
        mountainPrefabs = PrefabAutoLoader.EnsureLoaded(mountainPrefabs, "EnvPrefabs/Snow/Mountains");
        snowDriftPrefabs = PrefabAutoLoader.EnsureLoaded(snowDriftPrefabs, "EnvPrefabs/Snow/SnowDrifts");
        pineTreePrefabs = PrefabAutoLoader.EnsureLoaded(pineTreePrefabs, "EnvPrefabs/Snow/PineTrees");
        snowRockPrefabs = PrefabAutoLoader.EnsureLoaded(snowRockPrefabs, "EnvPrefabs/Snow/Rocks");
        logPrefabs = PrefabAutoLoader.EnsureLoaded(logPrefabs, "EnvPrefabs/Snow/Logs");
        propPrefabs = PrefabAutoLoader.EnsureLoaded(propPrefabs, "EnvPrefabs/Snow/Props");
        snowPilePrefabs = PrefabAutoLoader.EnsureLoaded(snowPilePrefabs, "EnvPrefabs/Snow/SnowPiles");
    }

    void Start()
    {
        roadHalfWidth = 10f;

        if (tintObjects)
        {
            SpawnLayerTinted(mountainPrefabs, mountainCount, backgroundMinZ, backgroundMaxZ, true, snowTint, tintStrength);
            SpawnLayerTinted(snowDriftPrefabs, snowDriftCount, backgroundMaxZ, backgroundMaxZ + 20f, false, snowTint, tintStrength);
            SpawnLayerTinted(pineTreePrefabs, pineTreeCount, midMinZ, midMaxZ, true, snowTint, tintStrength);
            SpawnLayerTinted(snowRockPrefabs, snowRockCount, midMinZ, midMaxZ, true, snowTint, tintStrength);
            SpawnLayerTinted(logPrefabs, logCount, midMinZ, midMaxZ, true, snowTint, tintStrength);
            SpawnLayerTinted(propPrefabs, propCount, nearMinZ, nearMaxZ, true, snowTint, tintStrength);
            SpawnLayerTinted(snowPilePrefabs, snowPileCount, nearMinZ, nearMaxZ, false, snowTint, tintStrength);
        }
        else
        {
            SpawnLayer(mountainPrefabs, mountainCount, backgroundMinZ, backgroundMaxZ, true);
            SpawnLayer(snowDriftPrefabs, snowDriftCount, backgroundMaxZ, backgroundMaxZ + 20f, false);
            SpawnLayer(pineTreePrefabs, pineTreeCount, midMinZ, midMaxZ, true);
            SpawnLayer(snowRockPrefabs, snowRockCount, midMinZ, midMaxZ, true);
            SpawnLayer(logPrefabs, logCount, midMinZ, midMaxZ, true);
            SpawnLayer(propPrefabs, propCount, nearMinZ, nearMaxZ, true);
            SpawnLayer(snowPilePrefabs, snowPileCount, nearMinZ, nearMaxZ, false);
        }
    }
}
