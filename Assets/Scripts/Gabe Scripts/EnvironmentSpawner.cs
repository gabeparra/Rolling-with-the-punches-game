using UnityEngine;

/// <summary>
/// Base class for theme-specific environment spawners.
/// Provides shared SpawnLayer and TintRenderers logic.
/// </summary>
public abstract class EnvironmentSpawner : MonoBehaviour
{
    [Header("Track Settings")]
    public float trackLength = 200f;
    public float trackCenterZ = 10.2f;

    [Header("Road Exclusion")]
    public float roadHalfWidth = 12f;

    [Header("Scale Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.3f;

    protected void SpawnLayer(GameObject[] prefabs, int count, float minZ, float maxZ, bool randomRotation)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        float effectiveMinZ = Mathf.Max(minZ, roadHalfWidth);
        // Skip layer entirely if the road exclusion covers this zone
        if (effectiveMinZ >= maxZ) return;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-trackLength / 2f, trackLength / 2f);

            float side = Random.value > 0.5f ? 1f : -1f;
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

    protected void SpawnLayerTinted(GameObject[] prefabs, int count, float minZ, float maxZ, bool randomRotation, Color tint, float tintStrength)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        float effectiveMinZ = Mathf.Max(minZ, roadHalfWidth);
        // Skip layer entirely if the road exclusion covers this zone
        if (effectiveMinZ >= maxZ) return;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-trackLength / 2f, trackLength / 2f);

            float side = Random.value > 0.5f ? 1f : -1f;
            float z = trackCenterZ + side * Random.Range(effectiveMinZ, maxZ);

            Vector3 pos = new Vector3(x, 0f, z);
            Quaternion rot = randomRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : Quaternion.identity;

            GameObject obj = Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, rot, transform);

            float scale = Random.Range(minScale, maxScale);
            obj.transform.localScale *= scale;

            TintRenderers(obj, tint, tintStrength);
        }
    }

    protected static void TintRenderers(GameObject obj, Color tint, float strength)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                Color original = mat.color;
                mat.color = Color.Lerp(original, tint, strength);
            }
        }
    }
}
