using UnityEngine;

public class WorldScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 10f;
    public float maxDistance = 650f;
    public float destroyBehindDistance = 50f;

    [Header("References")]
    public Transform trainTransform;

    private float totalDistanceTraveled = 0f;
    private bool stopped = false;

    public bool IsStopped => stopped;
    public float TotalDistanceTraveled => totalDistanceTraveled;

    void FixedUpdate()
    {
        if (stopped) return;

        float step = scrollSpeed * Time.fixedDeltaTime;
        totalDistanceTraveled += step;

        if (totalDistanceTraveled >= maxDistance)
        {
            stopped = true;
            return;
        }

        // Move world backward, then sync physics so child trigger colliders
        // are recognized at their new positions (fires OnTriggerEnter on tracks)
        transform.position += Vector3.left * step;
        Physics.SyncTransforms();
    }

    void Update()
    {
        if (trainTransform == null) return;

        float trainX = trainTransform.position.x;

        // Only clean up dynamically spawned track pieces that have scrolled far behind
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            // Skip non-track children (environment spawners, static objects, etc.)
            if (child.GetComponentInChildren<TrackSpawner>() == null) continue;

            if (child.position.x < trainX - destroyBehindDistance)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
