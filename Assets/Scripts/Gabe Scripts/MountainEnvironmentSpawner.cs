using UnityEngine;

public class MountainEnvironmentSpawner : MonoBehaviour
{
    [Header("Track Settings")]
    public float trackLength = 200f;
    public float trackCenterZ = 0f;

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

    [Header("Road Exclusion")]
    public float roadHalfWidth = 12f;

    [Header("Scale Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.3f;

    [Header("Mountain Tint")]
    public bool tintObjects = true;
    public Color mountainTint = new Color(0.5f, 0.48f, 0.45f);

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
        SpawnLayer(peakPrefabs, peakCount, backgroundMinZ, backgroundMaxZ, true);
        SpawnLayer(cliffWallPrefabs, cliffWallCount, backgroundMaxZ, backgroundMaxZ + 20f, false);
        SpawnLayer(boulderPrefabs, boulderCount, midMinZ, midMaxZ, true);
        SpawnLayer(treePrefabs, treeCount, midMinZ, midMaxZ, true);
        SpawnLayer(bushPrefabs, bushCount, midMinZ, midMaxZ, true);
        SpawnLayer(propPrefabs, propCount, nearMinZ, nearMaxZ, true);
        SpawnLayer(grassPrefabs, grassCount, nearMinZ, nearMaxZ, false);
    }

    void SpawnLayer(GameObject[] prefabs, int count, float minZ, float maxZ, bool randomRotation)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-trackLength / 2f, trackLength / 2f);

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

            if (tintObjects)
                TintRenderers(obj);
        }
    }

    void TintRenderers(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                Color original = mat.color;
                mat.color = Color.Lerp(original, mountainTint, 0.45f);
            }
        }
    }
}
