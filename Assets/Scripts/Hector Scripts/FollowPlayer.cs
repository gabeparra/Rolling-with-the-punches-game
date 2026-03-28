using UnityEngine;

public class FollowTrainScript : MonoBehaviour
{
    public Transform trainTransform;   // forward movement
    public Transform playerTransform;  // side-to-side movement

    public Vector3 offset;
    [Header("Camera Tuning")]
    public float leftOffset = -3f;     
    public float smoothSpeed = 8f;     
    
    void Start()
    {
        if (offset == Vector3.zero && trainTransform != null)
        {
            offset = transform.position - trainTransform.position;
        }
    }

    void LateUpdate()
    {
        if (trainTransform != null && playerTransform != null)
        {
            // Train is stationary — follow player position for both axes
            Vector3 targetPos = new Vector3(
                playerTransform.position.x + offset.x + leftOffset,
                transform.position.y,
                playerTransform.position.z + offset.z
            );
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }
    }
}
