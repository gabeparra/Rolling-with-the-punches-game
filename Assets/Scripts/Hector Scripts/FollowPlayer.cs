using UnityEngine;
/*
    Originally designed for camera to follow the train back when is on a vector,
    this script is now forcing the camera to follow the player -- I should probably use cinemachine instead *shrug emoji*
*/
public class FollowPlayer : MonoBehaviour
{
    public Transform trainTransform;   // forward movement
    public Transform playerTransform;  // side-to-side movement

    public Vector3 offset; // This creates a visual offset from camera position 
    public float leftOffset = -3f; // Camera starts offset left a little bit 
    public float smoothSpeed = 8f; // Helps make visual motion a little smoother

    void Start()
    {
        if (offset == Vector3.zero && trainTransform != null) 
        {
            offset = transform.position - trainTransform.position; // This takes the initial difference of the train's position w/ cam
        }
    }

    void LateUpdate()
    {
        if (trainTransform != null && playerTransform != null)
        {
            Vector3 targetPos = new Vector3( // This block here is what is actually updating to move the camera to follow
                playerTransform.position.x + offset.x + leftOffset,
                transform.position.y,
                trainTransform.position.z + offset.z
            );
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime); // using interpolation for smoothness
        }
    }
}
