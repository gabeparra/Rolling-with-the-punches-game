using UnityEngine;
using UnityEngine.AI;

public class ImprovedBanditFSM : BanditFSM
{
    public float detectionRange = 25f;
    public float attackRange = 18f;

    NavMeshAgent myAgent;
    Rigidbody rb;
    Animator animator;
    SmartSearchState smartSearchState;
    Enemy_AI enemyAI;
    int lastSyncedHealth = -1;
    float landedBodyY = float.MinValue;
    bool hasLanded = false;

    new void Awake()
    {
        parent = gameObject;
        myAgent = parent.GetComponent<NavMeshAgent>();
        // Disable NavMeshAgent immediately to prevent it from snapping the enemy
        // to the ground NavMesh (far below the train) during the frame before
        // the spawner can disable it. Re-enable in Start() if actually on a NavMesh.
        if (myAgent != null) myAgent.enabled = false;
        rb = parent.GetComponent<Rigidbody>();
        animator = parent.GetComponentInChildren<Animator>();
        target = parent.transform.position;

        // Find player by tag if not assigned
        if (enemy_target == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) enemy_target = found;
        }

        // Create improved states
        smartSearchState = parent.AddComponent<SmartSearchState>();
        SmartLootState smartLootState = parent.AddComponent<SmartLootState>();
        SmartAttackState smartAttackState = parent.AddComponent<SmartAttackState>();

        states = new()
        {
            { "loot", smartLootState },
            { "attack", smartAttackState },
            { "search", smartSearchState }
        };

        foreach (State state in states.Values)
        {
            state.Init(this, parent);
        }

        if (loot_targets_container)
        {
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();
        }

        // Cache Enemy_AI for health sync
        enemyAI = GetComponent<Enemy_AI>();
        if (enemyAI != null)
        {
            lastSyncedHealth = enemyAI.health;
        }
    }

    protected override void Start()
    {
        // Only set starting state if the spawner hasn't already set one.
        // The spawner calls SetCurrentState("attack") before Start() runs.
        if (current_state == null)
            SetCurrentState("loot");
    }

    protected override void Update()
    {
        // Sync floor reference to smart search state
        smartSearchState.floor = floor;

        // Sync health from Enemy_AI (since PlayerShooting damages Enemy_AI)
        SyncHealthFromEnemyAI();

        if (current_state == null) return;
        current_state.StateUpdate();

        // Drive walk animation
        if (animator != null)
        {
            if (myAgent != null && myAgent.enabled && myAgent.isOnNavMesh)
                animator.SetBool("isMoving", myAgent.velocity.sqrMagnitude > 0.1f);
            else
                animator.SetBool("isMoving", (target - parent.transform.position).sqrMagnitude > 0.5f);
        }
    }

    protected override void FixedUpdate()
    {
        // Drive movement — use NavMeshAgent if available, otherwise simple movement
        if (myAgent != null && target != parent.transform.position)
        {
            if (myAgent.enabled && myAgent.isOnNavMesh)
            {
                myAgent.destination = target;
            }
            else if (rb != null && !rb.isKinematic)
            {
                // Velocity-based movement when NavMeshAgent is disabled (e.g. on the train).
                float moveSpeed = myAgent.speed > 0 ? myAgent.speed : 3.5f;
                Vector3 dir = (target - parent.transform.position);
                dir.y = 0f;

                // Clamp downward velocity when grounded to prevent sinking
                float yVel = rb.linearVelocity.y;
                if (yVel < 0f && IsGrounded())
                    yVel = 0f;

                if (dir.sqrMagnitude > 0.25f)
                {
                    Vector3 horizontal = dir.normalized * moveSpeed;
                    rb.linearVelocity = new Vector3(horizontal.x, yVel, horizontal.z);
                    rb.MoveRotation(Quaternion.LookRotation(dir.normalized));
                }
                else
                {
                    rb.linearVelocity = new Vector3(0f, yVel, 0f);
                }
            }
        }

        if (current_state == null) return;
        current_state.StateFixedUpdate();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if (!hasLanded)
        {
            if (IsGrounded())
            {
                hasLanded = true;
                landedBodyY = animator.bodyPosition.y;
            }
            return;
        }

        float currentBodyY = animator.bodyPosition.y;
        float drift = landedBodyY - currentBodyY;
        if (drift > 0.01f)
        {
            Vector3 pos = parent.transform.position;
            parent.transform.position = new Vector3(pos.x, pos.y + drift, pos.z);
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(parent.transform.position + Vector3.up * 0.1f, Vector3.down, 0.4f);
    }

    void SyncHealthFromEnemyAI()
    {
        if (enemyAI == null) return;

        if (enemyAI.health != lastSyncedHealth)
        {
            int damageTaken = lastSyncedHealth - enemyAI.health;
            if (damageTaken > 0)
            {
                health -= damageTaken;
                // FIX: Don't call OnEnemyKilled or Destroy here.
                // Enemy_AI.Die() already handles kill counting, death effects,
                // and destruction. Doing it here too caused double-counting
                // which broke the enemy remaining HUD count.
                if (health <= 0)
                {
                    return;
                }

            }
            lastSyncedHealth = enemyAI.health;
        }
    }

    public bool CanSeePlayer()
    {
        if (enemy_target == null) return false;

        Vector3 origin = parent.transform.position + Vector3.up * 0.5f;
        Vector3 dir = enemy_target.transform.position - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, detectionRange))
        {
            return hit.collider.gameObject == enemy_target ||
                   hit.collider.transform.IsChildOf(enemy_target.transform);
        }
        return false;
    }

    public float DistanceToPlayer()
    {
        if (enemy_target == null) return float.MaxValue;
        return Vector3.Distance(parent.transform.position, enemy_target.transform.position);
    }
}
