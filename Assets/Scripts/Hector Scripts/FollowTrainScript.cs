/*
using UnityEngine;

public class FollowTrainScript : MonoBehaviour
{

    public Transform trainTransform; // Used to define train's location
    public Vector3 offset; // Value for how far camera sits away from the train

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // A little redundency here to ensure cam distance is maintained relative to train
        if (offset == Vector3.zero && trainTransform != null)
        {
            offset = transform.position - trainTransform.position;
        }
    }

    // Update is called once per frame
    // I am using lateUpdate here to help mitigate camera jitter
    void LateUpdate()
    {
        if (trainTransform != null)
        {
            // Moves camera to the x-pos of the the train plus the given offset value
            // Camera's original y and z positions are maintained relative to the camera, not the train
            transform.position = new Vector3(trainTransform.position.x + offset.x, transform.position.y, trainTransform.position.z + offset.z);
        }
    }
}
*/