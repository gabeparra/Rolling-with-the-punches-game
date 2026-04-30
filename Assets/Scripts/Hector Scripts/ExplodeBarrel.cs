using System.Collections;
using UnityEngine;

public class ExplodeBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionEffect;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;

    private bool _hasExploded = false;

    // Start is called once before the first execution of Update
    void Start()
    {
        // You could use this to randomize the barrel's size or rotation
    }

    // Update is called once per frame
    void Update()
    {
        // You could use this to make the barrel spin or hover
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If hit by a bullet and we haven't blown up yet...
        if (collision.gameObject.CompareTag("Bullet") && !_hasExploded)
        {
            Explode();
        }
    }

    public void TakeDamage(int amount)
    {
        if (_hasExploded) return;
        Explode();
    }

    void Explode()
    {
        _hasExploded = true;

        // 1. Trigger the visual "Boom" (auto-destroy after 3s as safety)
        if (explosionEffect != null)
        {
            GameObject vfx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        // 2. Knockback - only Player and Enemies (props are ignored)
        Collider[] victims = Physics.OverlapSphere(transform.position, explosionRadius);
        var pushed = new System.Collections.Generic.HashSet<Rigidbody>();
        foreach (Collider victim in victims)
        {
            bool isPlayer = victim.CompareTag("Player") || victim.GetComponentInParent<TrainPlayerController>() != null;
            bool isEnemy = victim.GetComponentInParent<Bandit>() != null;
            if (!isPlayer && !isEnemy) continue;

            Rigidbody rb = victim.GetComponentInParent<Rigidbody>();
            if (rb == null || pushed.Contains(rb)) continue;
            pushed.Add(rb);
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        // 3. Hide the barrel immediately
        var mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 4. Start the despawn countdown
        StartCoroutine(DespawnRoutine());
    }

    IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}