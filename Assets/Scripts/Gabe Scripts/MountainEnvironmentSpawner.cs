using UnityEngine;

public class MountainEnvironmentSpawner : EnvironmentSpawner
{
    [Header("Background (far from track)")]
    public GameObject[] peakPrefabs;
    public GameObject[] cliffWallPrefabs;
    public int peakCount = 10;
    public int cliffWallCount = 6;
    public float backgroundMinZ = 40f;
    public float backgroundMaxZ = 80f;

    [Header("Mid Range")]
    public GameObject[] boulderPrefabs;
    public GameObject[] treePrefabs;
    public GameObject[] bushPrefabs;
    public int boulderCount = 25;
    public int treeCount = 20;
    public int bushCount = 15;
    public float midMinZ = 10f;
    public float midMaxZ = 40f;

    [Header("Near Track (decoration)")]
    public GameObject[] propPrefabs;
    public GameObject[] grassPrefabs;
    public int propCount = 15;
    public int grassCount = 35;
    public float nearMinZ = 3f;
    public float nearMaxZ = 10f;

    [Header("Mountain Tint")]
    public bool tintObjects = true;
    public Color mountainTint = new Color(0.5f, 0.48f, 0.45f);
    public float tintStrength = 0.45f;

    void Awake()
    {
        peakPrefabs = PrefabAutoLoader.EnsureLoaded(peakPrefabs, "EnvPrefabs/Mountain/Peaks");
        cliffWallPrefabs = PrefabAutoLoader.EnsureLoaded(cliffWallPrefabs, "EnvPrefabs/Mountain/CliffWalls");
        boulderPrefabs = PrefabAutoLoader.EnsureLoaded(boulderPrefabs, "EnvPrefabs/Mountain/Boulders");
        treePrefabs = PrefabAutoLoader.EnsureLoaded(treePrefabs, "EnvPrefabs/Mountain/Trees");
        bushPrefabs = PrefabAutoLoader.EnsureLoaded(bushPrefabs, "EnvPrefabs/Mountain/Bushes");
        propPrefabs = PrefabAutoLoader.EnsureLoaded(propPrefabs, "EnvPrefabs/Mountain/Props");
        grassPrefabs = PrefabAutoLoader.EnsureLoaded(grassPrefabs, "EnvPrefabs/Mountain/Grass");
    }

    void Start()
    {
        roadHalfWidth = 10f;

        if (tintObjects)
        {
            SpawnLayerTinted(peakPrefabs, peakCount, backgroundMinZ, backgroundMaxZ, true, mountainTint, tintStrength);
            SpawnLayerTinted(cliffWallPrefabs, cliffWallCount, backgroundMaxZ, backgroundMaxZ + 20f, false, mountainTint, tintStrength);
            SpawnLayerTinted(boulderPrefabs, boulderCount, midMinZ, midMaxZ, true, mountainTint, tintStrength);
            SpawnLayerTinted(treePrefabs, treeCount, midMinZ, midMaxZ, true, mountainTint, tintStrength);
            SpawnLayerTinted(bushPrefabs, bushCount, midMinZ, midMaxZ, true, mountainTint, tintStrength);
            SpawnLayerTinted(propPrefabs, propCount, nearMinZ, nearMaxZ, true, mountainTint, tintStrength);
            SpawnLayerTinted(grassPrefabs, grassCount, nearMinZ, nearMaxZ, false, mountainTint, tintStrength);
        }
        else
        {
            SpawnLayer(peakPrefabs, peakCount, backgroundMinZ, backgroundMaxZ, true);
            SpawnLayer(cliffWallPrefabs, cliffWallCount, backgroundMaxZ, backgroundMaxZ + 20f, false);
            SpawnLayer(boulderPrefabs, boulderCount, midMinZ, midMaxZ, true);
            SpawnLayer(treePrefabs, treeCount, midMinZ, midMaxZ, true);
            SpawnLayer(bushPrefabs, bushCount, midMinZ, midMaxZ, true);
            SpawnLayer(propPrefabs, propCount, nearMinZ, nearMaxZ, true);
            SpawnLayer(grassPrefabs, grassCount, nearMinZ, nearMaxZ, false);
        }
    }
}
