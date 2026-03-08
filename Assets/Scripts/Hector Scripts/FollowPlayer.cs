using UnityEngine;

public class FollowTrainScript : MonoBehaviour
{
    public Transform trainTransform;   // forward movement
    public Transform playerTransform;  // side-to-side movement

    public Vector3 offset;

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
            transform.position = new Vector3(
                playerTransform.position.x + offset.x,  // follow player sideways
                transform.position.y,                   // keep camera height
                trainTransform.position.z + offset.z    // follow train forward
            );
        }
    }
}
