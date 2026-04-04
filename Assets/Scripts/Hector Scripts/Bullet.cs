using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    private Vector3 _target;
    private float _speed;
    private float _timeout = 3f;
    private bool _visualOnly;

    public void Init(Vector3 origin, Vector3 hitPoint, float tracerSpeed)
    {
        transform.position = origin;
        _target = hitPoint;
        _speed = tracerSpeed;
        _visualOnly = true;
        transform.LookAt(_target);
    }

    void Update()
    {
        _timeout -= Time.deltaTime;
        if (_timeout <= 0f) { Destroy(gameObject); return; }

        if (_visualOnly)
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _target) < 0.05f)
                Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (CompareTag("EnemyBullet"))
        {
            PlayerHealth ph = collision.collider.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(1);
        }
        Destroy(gameObject);
    }
}