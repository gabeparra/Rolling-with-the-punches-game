using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;

public class Bandit : MonoBehaviour
{
    GameObject parent;
    private BanditFSM fsm;

    public float health = 2f;

    // NOTE: Cover appended at end so existing serialized defaultState values
    // (Idle=0/Attack=1/Loot=2) keep meaning the same in saved scenes/prefabs.
    public enum State {Idle, Attack, Loot, Cover};

    public State defaultState = State.Idle;

    // ---- Cover behavior (CoverState) ----
    [Tooltip("If true, healthy bandits break off attacks to seek cover when they take fire.")]
    public bool useCover = true;
    [Tooltip("How recently the bandit must have taken a hit to count as 'under fire'.")]
    public float coverFearWindow = 1.75f;
    [Tooltip("Minimum health fraction to bother taking cover.")]
    [Range(0f, 1f)]
    public float coverHealthFloor = 0.35f;
    [Tooltip("Cooldown after leaving cover before re-entering.")]
    public float coverCooldown = 4f;

    [HideInInspector] public float lastDamagedAt = -1000f;
    [HideInInspector] public float lastLeftCoverAt = -1000f;
    [HideInInspector] public float maxHealth = 0f;

    public float change_to_loot_state_chance = 0.15f;

    // OPTIONAL TIME LOOTED PARAMETERS
    public float time_looted = 0f;

    [Range(1,30)]
    public float max_looting_time = 10f;

    public enum LootFollowType {Closest, Random};

    public LootFollowType loot_follow_type = LootFollowType.Random;

    public float loot_take_percentage = .2f;

    public GameObject bullet_prefab;

    public GameObject enemy_target;

    public GameObject loot_targets_container;

    public Transform[] loot_targets;

    public GameObject floor;

    public Transform target;

    public NavMeshAgent agent;

    private Animator animator;
    private UnityEngine.Vector3 prevPos;


    // LEVEL
    [Range(1,100)]
    public int level = 1;

    // SHOOT INTERVAL
    public float shoot_interval_level_increment = -.5f;

    [Range(0.1f,float.PositiveInfinity)]
    public float base_shoot_interval = 3.5f;

    [HideInInspector]
    public float shoot_interval;

    // SHOOT FORCE
    public float shoot_force_level_increment = 500f;

    [Range(1,20000)]
    public float base_shoot_force = 2000f;

    [HideInInspector]
    public float shoot_force;

    // MAG SIZE
    public int max_mag_size_level_increment = 1;

    public int base_max_mag_size = 6;

    [HideInInspector]
    public int max_mag_size;

    [HideInInspector]
    public int mag_size;


    // RELOAD TIME
    public float reload_time_level_increment = -.3f;

    public float base_reload_time = 2f;

    [HideInInspector]
    public float reload_time;


    // ACCURACY
    public float accuracy_level_increment = .15f;

    public float base_accuracy = .25f;

    [HideInInspector]
    public float accuracy;





    void Awake()
    {
        
    }

    public AudioClip deathClip;

    public void InitializeStats()
    {
        max_mag_size = base_max_mag_size + (level-1) * max_mag_size_level_increment;
        mag_size = max_mag_size; // Start with a full mag so the first shot doesn't wait for reload.
        accuracy = base_accuracy + (level-1) * accuracy_level_increment;
        reload_time = base_reload_time + (level-1) * reload_time_level_increment;
        shoot_force = base_shoot_force + (level-1) * shoot_force_level_increment;
        shoot_interval = base_shoot_interval + (level-1) * shoot_interval_level_increment;
        PrintStats();
    }

    void PrintStats()
    {
        print("ENEMY STATS AT LEVEL " + level);
        print("Max Mag Size: " + max_mag_size);
        print("Accuracy: " + accuracy);
        print("Reload Time: " + reload_time);
        print("Shoot Force: " + shoot_force);
        print("Shoot Inverval: " + shoot_interval);

        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parent = gameObject;
        agent = parent.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        prevPos = parent.transform.position;
        target = parent.transform;
        fsm = parent.AddComponent<BanditFSM>();
        fsm.Init(this);

        if (loot_targets_container)
        {
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>().Where(c => c.gameObject != loot_targets_container).ToArray();
        }

        InitializeStats();
        PrintStats();

        // foreach (var key in fsm.states.Keys)
        // {
        //     Debug.Log(key);
        // }

        // Stash starting health for CoverState health-fraction comparisons.
        maxHealth = health;

        switch (defaultState)
        {
            case State.Idle:
                fsm.SetCurrentState("idle");
                break;
            case State.Loot:
                fsm.SetCurrentState("loot");
                break;
            case State.Attack:
                fsm.SetCurrentState("attack");
                break;
            case State.Cover:
                fsm.SetCurrentState("cover");
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        fsm.DoUpdate();

        if (animator != null)
        {
            UnityEngine.Vector3 cur = parent.transform.position;
            UnityEngine.Vector3 delta = cur - prevPos;
            delta.y = 0f;
            // Threshold scaled to per-second movement; covers both navmesh
            // velocity and rigidbody-velocity bandits without false-positives
            // from physics jitter.
            bool isMoving = delta.sqrMagnitude / Mathf.Max(Time.deltaTime, 0.0001f) > 0.05f;
            animator.SetBool("isMoving", isMoving);
            prevPos = cur;
        }
    }

    void FixedUpdate()
    {
        fsm.DoFixedUpdate();
    }

    void Die() // Method added by Hector to destroy enemy on death
    {
        if (HUDManager.Instance != null) HUDManager.Instance.OnEnemyKilled();
        // Play death sound at the bandit's position (independent of the destroyed gameObject)
        if (deathClip != null)
            AudioSource.PlayClipAtPoint(deathClip, parent.transform.position, 1f);

        var fxPos = parent.transform.position + UnityEngine.Vector3.up * 1f;
        EnemyDeathFX.Play(fxPos, new Color(1f, 0.55f, 0.15f));

        Destroy(parent);
    }

    public void TakeDamage(int amount) // Method added by Hector to calculate damage taken by enemy
    {
        health -= amount;
        // CoverState's transition gate polls this to detect "under fire" recently.
        lastDamagedAt = Time.time;

        if (health <= 0)
        {
            Die();
        }
    }
}
