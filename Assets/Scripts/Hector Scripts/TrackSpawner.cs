using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public GameObject spawningTracks; // Drag your prefab here
    public Transform spawnPoint;      // Drag the child empty object here
    public int maxTracks = 10;
    
    private static int currentTrackCount = 0;
    private bool hasSpawned = false;

    void Start()
    {
    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        // Change "Train" to whatever your train object is named
        if (other.gameObject.name == "Train Engine 2" && !hasSpawned)
        {
            if (currentTrackCount < maxTracks)
            {
                currentTrackCount++;
                hasSpawned = true;
                Instantiate(spawningTracks, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
