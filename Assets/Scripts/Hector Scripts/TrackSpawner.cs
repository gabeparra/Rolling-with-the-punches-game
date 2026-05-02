using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public GameObject spawningTracks;
    public Transform spawnPoint;
    public int maxTracks = 10;

    private static int currentTrackCount = 0;
    private bool hasSpawned = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => currentTrackCount = 0;

    public static void ResetCounter() => currentTrackCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train") && !hasSpawned)
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
