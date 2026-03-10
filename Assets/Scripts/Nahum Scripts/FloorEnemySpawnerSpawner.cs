using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FloorEnemySpawnerSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    Vector3 spawn_position;
    Vector3 spawn_dir;
    public bool active = false;
    int max_enemies = 5;
    int enemies_spawned = 0;
    public GameObject loot_targets_container;
    Transform[] loot_targets;
    private List<GameObject> enemy_list = new List<GameObject>();
    public GameObject player;
    float spawnInterval = 3f;
    int maxFighters = 2;

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
        int fighterCount = 0;
        if (enemy_list.Count<1) { return; }

        for (int i=0; i<enemy_list.Count; i++)
        {
            GameObject enemy = enemy_list[i];
            Enemy_AI ai = enemy.GetComponent<Enemy_AI>();
            if (ai.state==Enemy_AI.State.FIGHT)
            {
                fighterCount++;
            }
        }
        //Debug.Log(fighterCount);

        if (fighterCount<maxFighters)
        {
            GameObject enemy = enemy_list[Random.Range(0, enemy_list.Count)];
            Enemy_AI ai = enemy.GetComponent<Enemy_AI>();
            ai.state = Enemy_AI.State.FIGHT;
            Debug.Log("Ensured a Fighter");
        }
    }

    int getFighterCOunt()
    {
        int fighterCount = 0;
        for (int i = 0; i < enemy_list.Count; i++)
        {
            GameObject enemy = enemy_list[i];
            Enemy_AI ai = enemy.GetComponent<Enemy_AI>();
            if (ai.state == Enemy_AI.State.FIGHT)
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
            enemy.transform.localScale = Vector3.one; // Force it to scale -- change added by Hector to stop shrink on spawn
            enemy.GetComponent<Enemy_AI>().state = Enemy_AI.State.LOOT;
            enemy_list.Add(enemy);
            enemy.GetComponent<Enemy_AI>().player = player;
            enemy.GetComponent<Enemy_AI>().enemy_list = enemy_list;
            enemy.GetComponent<Enemy_AI>().loot_targets_container = loot_targets_container;
            enemies_spawned++;
        }
    }
}
