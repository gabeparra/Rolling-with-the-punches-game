using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FloorEnemySpawnerSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    Vector3 spawn_position;
    Vector3 spawn_dir;
    public bool active = true;
    public int max_enemies = 5;
    int enemies_spawned = 0;
    public GameObject loot_targets_container;
    Transform[] loot_targets;
    private List<GameObject> enemy_list = new List<GameObject>();
    public GameObject player;
    public float spawnInterval = 3f;
    int maxFighters = 2;

    // ---- Leveled stats (Nahum's enemy stat config) ----
    public int level = 1;

    public float enemy_accuracy_level_increment = .15f;
    public float base_enemy_accuracy = .3f;

    public float max_health_level_increment = 2f;

    public float reload_time_level_increment = -.15f;
    public float shoot_interval_level_increment = -.15f;
    public float base_shoot_interval = 1.5f;
    public float base_reload_time = 2f;

    public int base_max_health = 6;

    public int base_max_mag_size = 6;
    public int max_mag_size_level_increment = 1;

    public float base_shoot_force = 2000f;
    public float shoot_force_level_increment = 100f;

    public float enemy_switch_to_looter_chance = .1f;
    public Bandit.LootFollowType loot_follow_type = Bandit.LootFollowType.Random;

    void Start()
    {
        if (!active) return;

        if (loot_targets_container == null)
        {
            Debug.LogError("[Spawner] loot_targets_container is NOT assigned in Inspector — bandits cannot find chests!", this);
        }
        else
        {
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();

            // Each Loot Target child doubles as a gold chest. Auto-attach so
            // bandits can find them via GoldChest.All.
            int attached = 0;
            foreach (Transform t in loot_targets)
            {
                if (t == loot_targets_container.transform) continue;
                if (t.GetComponent<GoldChest>() == null)
                {
                    t.gameObject.AddComponent<GoldChest>();
                    attached++;
                }
            }
            Debug.Log($"[Spawner] Loot targets: {loot_targets.Length - 1} children, attached {attached} GoldChest components, GoldChest.All count: {GoldChest.All.Count}");
        }

        InvokeRepeating("spawn", 2f, spawnInterval);
        InvokeRepeating("ensureFighters", 0f, spawnInterval * 2);

        if (HUDManager.Instance != null)
            HUDManager.Instance.SetTotalEnemies(max_enemies);
    }

    void ensureFighters()
    {
        enemy_list.RemoveAll(e => e == null);

        if (enemy_list.Count < 1) return;

        int fighterCount = getFighterCount();

        if (fighterCount < maxFighters)
        {
            // Only pull from non-looting bandits so chest-thieves stay on task.
            List<GameObject> candidates = new List<GameObject>();
            foreach (var e in enemy_list)
            {
                if (e == null) continue;
                BanditFSM fsm = e.GetComponent<BanditFSM>();
                if (fsm == null) continue;
                if (fsm.states.TryGetValue("loot", out var lootSt) && fsm.current_state == lootSt) continue;
                candidates.Add(e);
            }
            if (candidates.Count == 0) return;

            GameObject enemy = candidates[Random.Range(0, candidates.Count)];
            BanditFSM bfsm = enemy.GetComponent<BanditFSM>();
            if (bfsm != null)
                bfsm.SetCurrentState("attack");
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
            if (fsm != null && fsm.states.TryGetValue("attack", out var atk) && fsm.current_state == atk)
                fighterCount++;
        }
        return fighterCount;
    }

    void Update() { }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawn_position, 1);
        Gizmos.DrawLine(spawn_position, spawn_dir);
    }

    void spawn()
    {
        if (enemies_spawned >= max_enemies)
        {
            CancelInvoke("spawn");
            return;
        }
        Renderer renderer = GetComponent<Renderer>();
        Vector3 size = renderer.bounds.size;

        // Spawn slightly above the player — gravity settles them onto the deck.
        float sy = (player != null ? player.transform.position.y : transform.position.y) + 1f;
        float sx = Random.Range(transform.position.x - size.x / 2 + 1f, transform.position.x + size.x / 2 - 1f);
        float sz = Random.Range(transform.position.z - size.z / 2 + 1f, transform.position.z + size.z / 2 - 1f);
        spawn_position = new Vector3(sx, sy, sz);

        Vector3 dirToPlayer = (player != null ? player.transform.position - spawn_position : transform.forward);
        dirToPlayer.y = 0;
        spawn_dir = dirToPlayer.sqrMagnitude > 0.01f ? dirToPlayer.normalized : Vector3.forward;

        if (!enemyPrefab) return;

        Debug.Log("spawning enemy");
        GameObject enemy = Instantiate(enemyPrefab, spawn_position, Quaternion.LookRotation(spawn_dir, Vector3.up));

        // Parent to the train so enemies share the same physics hierarchy
        enemy.transform.SetParent(transform.root, true);

        // Keep world scale consistent even as the train rotates
        var scaler = enemy.AddComponent<MaintainWorldScale>();
        scaler.targetScale = 0.75f;

        // Disable NavMeshAgent — NavMesh is on the ground far below the train.
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = (RigidbodyConstraints)80; // FreezeRotationX | FreezeRotationZ
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Bandit ↔ player collision causes the player to get bumped through
        // the train deck. Ignore it physically so bandits can't push the player.
        Collider banditCol = enemy.GetComponent<Collider>();
        if (banditCol != null && player != null)
        {
            Collider playerCol = player.GetComponent<Collider>();
            if (playerCol != null) Physics.IgnoreCollision(banditCol, playerCol);
        }

        // Configure bandit stats via the Bandit stats component (Nahum's design).
        Bandit bandit = enemy.GetComponent<Bandit>();
        if (bandit != null)
        {
            bandit.defaultState = Bandit.State.Loot;
            bandit.change_to_loot_state_chance = enemy_switch_to_looter_chance;
            bandit.loot_follow_type = loot_follow_type;

            bandit.enemy_target = player;
            bandit.loot_targets_container = loot_targets_container;

            bandit.accuracy_level_increment = enemy_accuracy_level_increment;
            bandit.base_accuracy = base_enemy_accuracy;

            bandit.base_shoot_interval = base_shoot_interval;
            bandit.shoot_interval_level_increment = shoot_interval_level_increment;

            bandit.base_reload_time = base_reload_time;
            bandit.reload_time_level_increment = reload_time_level_increment;

            bandit.health = base_max_health + (level - 1) * max_health_level_increment;

            bandit.base_max_mag_size = base_max_mag_size;
            bandit.max_mag_size_level_increment = max_mag_size_level_increment;

            bandit.base_shoot_force = base_shoot_force;
            bandit.shoot_force_level_increment = shoot_force_level_increment;

            bandit.level = level;
            bandit.InitializeStats();
        }
        else
        {
            Debug.LogWarning("[Spawner] Spawned enemy missing Bandit component", enemy);
        }

        enemy_list.Add(enemy);
        enemies_spawned++;
    }
}
