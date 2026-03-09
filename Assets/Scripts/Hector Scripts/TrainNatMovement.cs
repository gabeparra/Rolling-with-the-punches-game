using UnityEngine;

public class TrainEngineConstant : MonoBehaviour
{
    public float trainSpeed = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure it's Kinematic so it doesn't fall through tracks
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        // Move the train forward along its own Z-axis
        if (rb.position.x < 650f) 
        {
            Vector3 newPosition = rb.position + transform.forward * trainSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }    
    }
}