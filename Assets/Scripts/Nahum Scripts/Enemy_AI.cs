using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy_AI : MonoBehaviour
{
    public GameObject head;
    public GameObject bulletPrefab;
    public int health = 6; // Added by Hector to set enemy health value
    public GameObject loot_targets_container;
    private Transform target;
    private NavMeshAgent agent;
    private Transform[] loot_targets;
    public List<GameObject> enemy_list;
    public GameObject player;
    const float detection_range = 50f;
    public float shootInterval = 3f;
    Vector3 shootOrigin;
    Vector3 shootDirection;
    float shootForce = 15;
    bool canSeePlayer = false;
    

    public enum  State{
        LOOT,
        FIGHT
    }
    public State state = State.LOOT;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        gameObject.tag = "Enemy";

        if (!player)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found;
        }

        if (loot_targets_container)
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();

        head = transform.Find("Head") ? transform.Find("Head").gameObject : gameObject;

        InvokeRepeating("shoot", 0, shootInterval);
    }

    public void TakeDamage(int amount) // Method added by Hector to calculate damage taken by enemy
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

void Die()
    {
        if (HUDManager.Instance != null) HUDManager.Instance.OnEnemyKilled();
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.collider.gameObject;
        if (obj)
        {
   }
    }

    void stopKnockback()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }


    private void OnDrawGizmos()
    {
        //head = transform.Find("Head") ? transform.Find("Head").gameObject : null;
        shootOrigin = head.transform.position;
        shootDirection = head.transform.forward;
        Gizmos.color = Color.indianRed;
        Gizmos.DrawRay(shootOrigin, shootDirection * shootForce);
    }

void shoot()
    {
        if (!canSeePlayer || state != State.FIGHT || !player) return;

        Vector3 origin = head.transform.position;
        Vector3 dirToPlayer = (player.transform.position - origin).normalized;

        // Instant raycast damage
        RaycastHit hit;
        if (Physics.Raycast(origin, dirToPlayer, out hit, detection_range))
        {
            PlayerHealth ph = hit.collider.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(1);
        }

        // Visual bullet -- spawned at origin, fired toward player
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(dirToPlayer));
            bullet.tag = "EnemyBullet";
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.linearVelocity = dirToPlayer * shootForce;
            }
            Destroy(bullet, 1f);
        }
    }

    // Update is called once per frame
void Update()
    {
        // Switch to FIGHT if player is within detection range
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= detection_range)
                state = State.FIGHT;
            else
                state = State.LOOT;
        }

        if (state == State.LOOT)
        {
            doLootProtocol();
            if (agent.velocity.magnitude != 0)
                transform.rotation = Quaternion.LookRotation(new Vector3(agent.velocity.normalized.x, 0, agent.velocity.normalized.z), Vector3.up);
        }
        else if (state == State.FIGHT)
        {
            doFightProtocol();
        }

        if (target == null)
            agent.SetDestination(transform.position);
    }

void doFightProtocol()
    {
        if (!player) return;

        Vector3 dirToPlayer = player.transform.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;

        // Always face the player
        Vector3 flatDir = new Vector3(dirToPlayer.x, 0, dirToPlayer.z);
        if (flatDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flatDir), 5f * Time.deltaTime);

        // Check line of sight
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detection_range))
            canSeePlayer = (hit.collider.gameObject == player || hit.collider.transform.IsChildOf(player.transform));
        else
            canSeePlayer = false;

        // Chase player if far, back off a little if too close
        float preferredRange = 6f;
        if (distToPlayer > preferredRange)
            agent.SetDestination(player.transform.position);
        else if (distToPlayer < preferredRange * 0.5f)
            agent.SetDestination(transform.position - flatDir.normalized * 2f);
        else
            agent.SetDestination(transform.position); // Hold position
    }

    void doLootProtocol()
    {
        if (target == null)
        {
            Transform closest = null;
            if (loot_targets == null) return;
            for (int i = 0; i < loot_targets.Length; i++)
            {
                Transform potential = loot_targets[i];
                if (!closest)
                {
                    closest = potential;
                }
                if (Vector3.Distance(closest.position, transform.position) > Vector3.Distance(potential.position, transform.position))
                {
                    closest = potential;
                }
            }
            target = closest;
        }

        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void OnDestroy()
    {
        if (enemy_list != null)
            enemy_list.Remove(this.gameObject);
    }
}
