using UnityEngine;

public class SnowyEnvironmentSpawner : MonoBehaviour
{
    [Header("Track Settings")]
    public float trackLength = 200f;
    public float trackCenterZ = 0f;

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

    [Header("Road Exclusion")]
    public float roadHalfWidth = 12f;

    [Header("Scale Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.3f;

    [Header("Snow Tint")]
    public bool tintObjects = true;
    public Color snowTint = new Color(0.85f, 0.88f, 0.95f);

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
        SpawnLayer(mountainPrefabs, mountainCount, backgroundMinZ, backgroundMaxZ, true);
        SpawnLayer(snowDriftPrefabs, snowDriftCount, backgroundMaxZ, backgroundMaxZ + 20f, false);
        SpawnLayer(pineTreePrefabs, pineTreeCount, midMinZ, midMaxZ, true);
        SpawnLayer(snowRockPrefabs, snowRockCount, midMinZ, midMaxZ, true);
        SpawnLayer(logPrefabs, logCount, midMinZ, midMaxZ, true);
        SpawnLayer(propPrefabs, propCount, nearMinZ, nearMaxZ, true);
        SpawnLayer(snowPilePrefabs, snowPileCount, nearMinZ, nearMaxZ, false);
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
                mat.color = Color.Lerp(original, snowTint, 0.55f);
            }
        }
    }
}
