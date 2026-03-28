using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public GameObject spawningTracks; // Drag your prefab here
    public Transform spawnPoint;      // Drag the child empty object here
    private bool hasSpawned = false;
    private Transform worldRoot;

    void Start()
    {
        WorldScroller scroller = FindFirstObjectByType<WorldScroller>();
        if (scroller != null)
            worldRoot = scroller.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Train Engine 2" && !hasSpawned)
        {
            hasSpawned = true;
            GameObject newTrack = Instantiate(spawningTracks, spawnPoint.position, spawnPoint.rotation);
            if (worldRoot != null)
                newTrack.transform.SetParent(worldRoot, true);
        }
    }
}
