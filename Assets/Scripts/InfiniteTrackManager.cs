using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiniteTrackManager : MonoBehaviour
{
    [Header("Train")]
    public Transform trainTransform;
    public float trainSpeed = 20f;

    [Header("Track Chunks")]
    public GameObject trackChunkPrefab;
    public float chunkLength = 146.4f;
    public int chunksAhead = 3;
    public int chunksBehind = 1;

    [Header("Ground")]
    public Transform groundTransform;

    [Header("Background (far from track)")]
    public GameObject[] buttePrefabs;
    public GameObject[] backgroundCardPrefabs;
    public int butteCountPerChunk = 2;
    public int backgroundCardCountPerChunk = 1;
    public float backgroundMinZ = 40f;
    public float backgroundMaxZ = 80f;

    [Header("Mid Range")]
    public GameObject[] cactusPrefabs;
    public GameObject[] deadTreePrefabs;
    public GameObject[] rockPrefabs;
    public int cactusCountPerChunk = 8;
    public int deadTreeCountPerChunk = 4;
    public int rockCountPerChunk = 5;
    public float midMinZ = 10f;
    public float midMaxZ = 40f;

    [Header("Near Track (decoration)")]
    public GameObject[] propPrefabs;
    public GameObject[] grassPrefabs;
    public int propCountPerChunk = 4;
    public int grassCountPerChunk = 10;
    public float nearMinZ = 3f;
    public float nearMaxZ = 10f;

    [Header("Scale Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.3f;

    private Dictionary<int, GameObject> spawnedChunks = new Dictionary<int, GameObject>();

    private float trainX;
    private float trainY;
    private float trainZ;
    private Quaternion trainRotation;

    private Material groundMaterial;
    private float groundStartX;

    void Start()
    {
        trainX = trainTransform.position.x;
        trainY = trainTransform.position.y;
        trainZ = trainTransform.position.z;
        trainRotation = trainTransform.rotation;

        Animator trainAnimator = trainTransform.GetComponent<Animator>();
        if (trainAnimator != null)
            trainAnimator.enabled = false;

        foreach (var sub in trainTransform.GetComponentsInChildren<StopSubanimations>())
            sub.enabled = false;
        foreach (var pause in trainTransform.GetComponentsInChildren<TrainMotionPause>())
            pause.enabled = false;

        if (groundTransform != null)
        {
            groundStartX = groundTransform.position.x;
            Renderer groundRenderer = groundTransform.GetComponent<Renderer>();
            if (groundRenderer != null)
                groundMaterial = groundRenderer.material;
        }

        UpdateChunks();
    }

    void LateUpdate()
    {
        trainX += trainSpeed * Time.deltaTime;

        trainTransform.position = new Vector3(trainX, trainY, trainZ);
        trainTransform.rotation = trainRotation;

        if (groundTransform != null)
        {
            groundTransform.position = new Vector3(trainX, groundTransform.position.y, groundTransform.position.z);

            if (groundMaterial != null)
            {
                float distanceMoved = trainX - groundStartX;
                float groundScale = groundTransform.localScale.x;
                float tiling = groundMaterial.mainTextureScale.x;
                float offset = -distanceMoved * tiling / groundScale;
                groundMaterial.mainTextureOffset = new Vector2(offset, 0f);
            }
        }

        UpdateChunks();
    }

    void UpdateChunks()
    {
        float trainX = trainTransform.position.x;
        int currentChunkIndex = Mathf.FloorToInt(trainX / chunkLength);

        int minIndex = currentChunkIndex - chunksBehind;
        int maxIndex = currentChunkIndex + chunksAhead;

        for (int i = minIndex; i <= maxIndex; i++)
        {
            if (!spawnedChunks.ContainsKey(i))
                SpawnChunk(i);
        }

        List<int> toRemove = new List<int>();
        foreach (var kvp in spawnedChunks)
        {
            if (kvp.Key < minIndex || kvp.Key > maxIndex)
                toRemove.Add(kvp.Key);
        }
        foreach (int index in toRemove)
        {
            Destroy(spawnedChunks[index]);
            spawnedChunks.Remove(index);
        }
    }

    void SpawnChunk(int chunkIndex)
    {
        float chunkStartX = chunkIndex * chunkLength;
        float trackY = trainTransform.position.y;
        float trackZ = trainTransform.position.z;

        GameObject chunkParent = new GameObject($"Chunk_{chunkIndex}");
        chunkParent.transform.position = new Vector3(chunkStartX, 0f, 0f);

        float trackRootX = chunkStartX + chunkLength;
        Instantiate(trackChunkPrefab, new Vector3(trackRootX, trackY, trackZ), Quaternion.identity, chunkParent.transform);

        StartCoroutine(SpawnEnvironmentAsync(chunkStartX, trackZ, chunkParent.transform));

        spawnedChunks[chunkIndex] = chunkParent;
    }

    IEnumerator SpawnEnvironmentAsync(float chunkStartX, float trackZ, Transform parent)
    {
        SpawnLayer(buttePrefabs, butteCountPerChunk, backgroundMinZ, backgroundMaxZ, true, chunkStartX, trackZ, parent);
        yield return null;
        if (parent == null) yield break;
        SpawnLayer(backgroundCardPrefabs, backgroundCardCountPerChunk, backgroundMaxZ, backgroundMaxZ + 20f, false, chunkStartX, trackZ, parent);
        SpawnLayer(cactusPrefabs, cactusCountPerChunk, midMinZ, midMaxZ, true, chunkStartX, trackZ, parent);
        yield return null;
        if (parent == null) yield break;
        SpawnLayer(deadTreePrefabs, deadTreeCountPerChunk, midMinZ, midMaxZ, true, chunkStartX, trackZ, parent);
        SpawnLayer(rockPrefabs, rockCountPerChunk, midMinZ, midMaxZ, true, chunkStartX, trackZ, parent);
        yield return null;
        if (parent == null) yield break;
        SpawnLayer(propPrefabs, propCountPerChunk, nearMinZ, nearMaxZ, true, chunkStartX, trackZ, parent);
        SpawnLayer(grassPrefabs, grassCountPerChunk, nearMinZ, nearMaxZ, false, chunkStartX, trackZ, parent);
    }

    void SpawnLayer(GameObject[] prefabs, int count, float minZ, float maxZ, bool randomRotation, float chunkStartX, float trackCenterZ, Transform parent)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            float x = chunkStartX + Random.Range(0f, chunkLength);

            float side = Random.value > 0.5f ? 1f : -1f;
            float z = trackCenterZ + side * Random.Range(minZ, maxZ);

            Vector3 pos = new Vector3(x, 0f, z);
            Quaternion rot = randomRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : Quaternion.identity;

            GameObject obj = Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, rot, parent);

            float scale = Random.Range(minScale, maxScale);
            obj.transform.localScale *= scale;
        }
    }
}
