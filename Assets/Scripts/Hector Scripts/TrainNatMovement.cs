using UnityEngine;

public class TrainEngineConstant : MonoBehaviour
{
    public float trainSpeed = 10f;
    public WorldScroller worldScroller;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // Forward speed to the world scroller instead of moving the train
        if (worldScroller != null)
        {
            worldScroller.scrollSpeed = trainSpeed;
        }
    }
}