using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject tracerPrefab;
    public int damage = 3;
    public Transform firePoint;
    public float tracerSpeed = 30f;
    public float maxRange = 50f;
    public AudioClip gunshotClip;
    public float reloadTime = 1f;
    private AudioSource audioSource;
    private bool _reloading = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_reloading)
        {
            if (HUDManager.Instance != null && !HUDManager.Instance.HasAmmo()) return;
            FireRaycast();
        }
        if (Input.GetKeyDown(KeyCode.R) && !_reloading)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        _reloading = true;
        yield return new WaitForSeconds(reloadTime);
        if (HUDManager.Instance != null) HUDManager.Instance.ReloadAmmo();
        _reloading = false;
    }

    void FireRaycast()
    {
        if (firePoint == null) return;
        if (HUDManager.Instance != null) HUDManager.Instance.SpendAmmo();
        if (gunshotClip != null) audioSource.PlayOneShot(gunshotClip);

        Vector3 origin = firePoint.position;
        Vector3 direction = transform.forward;
        Vector3 hitPoint;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRange))
        {
            hitPoint = hit.point;
            Enemy_AI enemyAI = hit.collider.GetComponentInParent<Enemy_AI>();
            EnemyScript enemyScript = hit.collider.GetComponentInParent<EnemyScript>();
            if (enemyAI != null) enemyAI.TakeDamage(damage);
            else if (enemyScript != null) enemyScript.TakeDamage(damage);
        }
        else
        {
            hitPoint = origin + direction * maxRange;
        }

        if (tracerPrefab != null)
        {
            GameObject tracer = Instantiate(tracerPrefab, origin, firePoint.rotation);
            BulletTracer bt = tracer.GetComponent<BulletTracer>();
            if (bt == null) bt = tracer.AddComponent<BulletTracer>();
            Rigidbody rb = tracer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            bt.Init(origin, hitPoint, tracerSpeed);
        }
    }
}