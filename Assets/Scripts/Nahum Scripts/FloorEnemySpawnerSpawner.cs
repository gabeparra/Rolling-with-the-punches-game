using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FloorEnemySpawnerSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    Vector3 spawn_position;
    Vector3 spawn_dir;
    public bool active = false;
    int max_enemies = 8;
    int enemies_spawned = 0;
    public GameObject loot_targets_container;
    Transform[] loot_targets;
    private List<GameObject> enemy_list = new List<GameObject>();
    public GameObject player;
    float spawnInterval = 5f;
    int maxFighters = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (active)
        {
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();
            InvokeRepeating("spawn", 2f, spawnInterval);
            InvokeRepeating("ensureFighters",0f,spawnInterval*2);

            if (HUDManager.Instance != null)
                HUDManager.Instance.SetTotalEnemies(max_enemies);
        }
    }

    void ensureFighters()
    {
        enemy_list.RemoveAll(e => e == null);

        if (enemy_list.Count < 1) { return; }

        int fighterCount = getFighterCount();

        if (fighterCount < maxFighters)
        {
            GameObject enemy = enemy_list[Random.Range(0, enemy_list.Count)];
            BanditFSM fsm = enemy.GetComponent<BanditFSM>();
            if (fsm != null)
                fsm.SetCurrentState("attack");
        }
    }

    int getFighterCount()
    {
        int fighterCount = 0;
        for (int i = 0; i < enemy_list.Count; i++)
        {
            GameObject enemy = enemy_list[i];
            if (enemy == null) continue;
            BanditFSM fsm = enemy.GetComponent<BanditFSM>();
            if (fsm != null && fsm.states["attack"] == fsm.current_state)
            {
                fighterCount++;
            }
        }
        return fighterCount;
    }

    // Update is called once per frame
    void Update()
    {
        
        //Debug.Log(fighterCount);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawn_position,1);
        Gizmos.DrawLine(spawn_position,spawn_dir);

    }

    void spawn()
    {
        // Stop spawning once we've hit the limit
        if (enemies_spawned >= max_enemies)
        {
            CancelInvoke("spawn");
            return;
        }
        Renderer renderer = GetComponent<Renderer>();
        Vector3 size = renderer.bounds.size;

        // Spawn slightly above the player — gravity settles them onto the deck.
        float sy = player.transform.position.y + 1f;
        // Keep enemies within the train bounds (inset by 1 unit from edges)
        float sx = Random.Range(transform.position.x - size.x / 2 + 1f, transform.position.x + size.x / 2 - 1f);
        float sz = Random.Range(transform.position.z - size.z / 2 + 1f, transform.position.z + size.z / 2 - 1f);
        spawn_position = new Vector3(sx, sy, sz);
        // Direction toward the player (flat)
        Vector3 dirToPlayer = (player.transform.position - spawn_position);
        dirToPlayer.y = 0;
        spawn_dir = dirToPlayer.sqrMagnitude > 0.01f ? dirToPlayer.normalized : Vector3.forward;
        if (enemyPrefab)
        {
            Debug.Log("spawning enemy");
            GameObject enemy = Instantiate(enemyPrefab, spawn_position, Quaternion.LookRotation(spawn_dir, Vector3.up));
            BanditFSM fsm = enemy.GetComponent<BanditFSM>();

            // Parent to the train so enemies share the same physics hierarchy
            enemy.transform.SetParent(transform.root, true);

            // Keep world scale consistent even as the train rotates
            var scaler = enemy.AddComponent<MaintainWorldScale>();
            scaler.targetScale = 0.75f;

            // Disable NavMeshAgent — the NavMesh is on the ground far below,
            // so the agent would snap enemies off the train. ImprovedBanditFSM
            // falls back to velocity-based movement when the agent is disabled.
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            // Match the player's Rigidbody setup: gravity on, not kinematic,
            // freeze rotation to prevent tumbling, same constraints as player.
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.constraints = (RigidbodyConstraints)80; // FreezeRotationX | FreezeRotationZ (matches player)
                rb.interpolation = RigidbodyInterpolation.Interpolate; // matches player
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // prevent tunneling through thin floor colliders
            }

            enemy_list.Add(enemy);

            if (fsm != null)
            {
                fsm.SetCurrentState("attack");
                fsm.enemy_target = player;
                fsm.loot_targets_container = loot_targets_container;
            }
            else
            {
                Debug.LogWarning($"[FloorEnemySpawnerSpawner] enemy_dummy prefab missing BanditFSM component", enemy);
            }
            enemies_spawned++;
        }
    }
}
