using UnityEngine;

// Purely visual tracer -- no Rigidbody, no physics.
// Moves toward a pre-calculated hit point then self-destructs.
public class BulletTracer : MonoBehaviour
{
    private Vector3 _target;
    private float _speed;
    private float _timeout = 5f;

    public void Init(Vector3 origin, Vector3 hitPoint, float tracerSpeed)
    {
        transform.position = origin;
        _target = hitPoint;
        _speed = tracerSpeed;
        transform.LookAt(_target);
    }

    void Update()
    {
        _timeout -= Time.deltaTime;
        if (_timeout <= 0f) { Destroy(gameObject); return; }

        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _target) < 0.05f)
            Destroy(gameObject);
    }
}