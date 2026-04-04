using UnityEngine;

public class WallMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float lifeSpan = 20.0f;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawned = false;

    void Start()
    {
        // Always capture world position at birth
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        lifeSpan = 20.0f; // Reset on each new instance
        hasSpawned = false;
    }

    void Update()
    {
        // Space.World ensures movement is always along world axis
        // regardless of parent or local rotation
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        lifeSpan -= Time.deltaTime;
        if (!hasSpawned && lifeSpan <= 1f)
        {
            Instantiate(gameObject, spawnPosition, spawnRotation);
            hasSpawned = true;
        }
        if (lifeSpan <= 0)
        {
            Destroy(gameObject);
        }
    }
}

