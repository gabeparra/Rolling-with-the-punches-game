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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (active)
        {
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();
            InvokeRepeating("spawn", 2f, spawnInterval);
            InvokeRepeating("ensureFighters",0f,spawnInterval*2);
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
            fsm.SetCurrentState("attack");
        }
    }

    int getFighterCount()
    {
        int fighterCount = 0;
        for (int i = 0; i < enemy_list.Count; i++)
        {
            GameObject enemy = enemy_list[i];
            BanditFSM fsm = enemy.GetComponent<BanditFSM>();
            if (fsm.states["attack"] == fsm.current_state)
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
        if (enemies_spawned > max_enemies) { return;}
        //Debug.Log("Setting spawner");
        Renderer renderer = GetComponent<Renderer>();
        Vector3 size = renderer.bounds.size;

        //float sx = Random.Range(0,2) == 0 ? transform.position.x - size.x/2 +1 : transform.position.x + size.x / 2 - 1;
        float sy = transform.position.y + 3; 
        float sz = Random.Range(0, 2) == 0 ? transform.position.z - size.z / 2 - 1 : transform.position.z + size.z / 2 + 1;
        float sx = Random.Range(transform.position.x - size.x / 2 + 2, transform.position.x + size.x / 2 - 2);
        //float sz = Random.Range(transform.position.z - size.z / 2 - 2, transform.position.z + size.z / 2 + 2); 
        spawn_position = new Vector3(sx,sy,sz);
        spawn_dir = spawn_position + new Vector3(transform.position.x > sx ? 3*1.5f : -3*1.5f,1f,0);
        if (enemyPrefab)
        {
            Debug.Log("spawning enemy");
            GameObject enemy = Instantiate(enemyPrefab, spawn_position, Quaternion.LookRotation(spawn_dir));
            BanditFSM fsm = enemy.GetComponent<BanditFSM>();
            enemy.transform.localScale = Vector3.one; // Force it to scale -- change added by Hector to stop shrink on spawn
            fsm.SetCurrentState("loot");
            enemy_list.Add(enemy);
            fsm.enemy_target = player;
            fsm.loot_targets_container = loot_targets_container;

            fsm.attackState.accuracy = base_enemy_accuracy + (level - 1) * enemy_accuracy_level_increment;
            fsm.attackState.shoot_interval = Mathf.Max(0.1f, base_shoot_interval + (level - 1) * shoot_interval_level_increment);
            fsm.attackState.reload_time = Mathf.Max(0.1f, base_reload_time + (level - 1) * reload_time_level_increment);
            fsm.health = base_max_health + (level - 1) * max_health_level_increment;
            fsm.attackState.max_mag_size = base_max_mag_size + (level - 1) * max_mag_size_level_increment;
            fsm.attackState.shoot_force = base_shoot_force + (level - 1) * shoot_force_level_increment;
            enemies_spawned++;
        }
    }
}
