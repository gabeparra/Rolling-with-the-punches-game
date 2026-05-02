using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField] private float smoothTime = 0.15f;
    private Vector3 offset;
    private Vector3 _velocity;

    void Start()
    {
        if (player != null)
            offset = transform.position - player.transform.position;
    }

    // BUG-34 fix: use SmoothDamp for framerate-independent smooth follow
    void LateUpdate()
    {
        if (player == null) return;
        Vector3 target = player.transform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, smoothTime);
    }
}
