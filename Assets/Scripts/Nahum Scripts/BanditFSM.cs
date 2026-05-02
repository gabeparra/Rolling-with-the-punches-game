// using System;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;

public class BanditFSM : FSM
{
    public Bandit bandit;

    // SearchState searchState;

    LootState lootState;

    AttackState attackState;

    IdleState idleState;

    CoverState coverState;


    public override void SetCurrentState(string state)
    {
        base.SetCurrentState(state);
        //current_state.SetTarget(target);
    }

    public void Init(Bandit bandit)
    {
        this.bandit = bandit;
        foreach (State state in states.Values)
        {
            state.Init(bandit, this, parent);
        }
    }

    void Awake()
    {
        parent = gameObject;
 

        // searchState = parent.AddComponent<SearchState>();
        // {"escape", parent.AddComponent<EscapeState>()},

        lootState = parent.AddComponent<LootState>();
        attackState = parent.AddComponent<AttackState>();
        idleState = parent.AddComponent<IdleState>();
        coverState = parent.AddComponent<CoverState>();

        states = new()
        {
        {"loot", lootState},
        {"attack", attackState},
        {"idle", idleState},
        {"cover", coverState}
        };

        foreach (State state in states.Values)
        {
            state.Init(bandit, this, parent);
        }
        
        InvokeRepeating("TryRandomSwitchToLooter",0,1f);

    }

    void TryRandomSwitchToLooter()
    {
        if (current_state!=attackState) {return;}

        float r = Random.Range(0,.9f);
        if (r <= bandit.change_to_loot_state_chance)
        {
            SetCurrentState("loot");
            print("randomly changed to looter: " + r);
        }
        else
        {
            print("failed to switch: " + r);
        }
    }

    protected override void Start()
    {
        base.Start();
    }


    public override void DoUpdate()
    {
        // searchState.floor = floor;

        if (current_state==null) {return;}

        // Attack → Cover transition. Four gates:
        //   1) Currently attacking
        //   2) Damaged within coverFearWindow (under fire)
        //   3) Health above coverHealthFloor (worth retreating)
        //   4) Past coverCooldown (no attack↔cover pingpong)
        if (current_state == attackState && bandit.useCover)
        {
            float sinceHit   = Time.time - bandit.lastDamagedAt;
            float sinceCover = Time.time - bandit.lastLeftCoverAt;
            float healthFrac = bandit.maxHealth > 0f ? bandit.health / bandit.maxHealth : 1f;
            if (sinceHit   <  bandit.coverFearWindow &&
                sinceCover >  bandit.coverCooldown   &&
                healthFrac >= bandit.coverHealthFloor)
            {
                SetCurrentState("cover");
            }
        }

        // Revert to attack after no more good can be taken
        if (current_state==lootState)
        {
            if (GameManager.getCurrency() <= 0)
            {
                SetCurrentState("attack");
            }
        }

        // Revert to attack after spent enough time looting
        if (current_state==lootState)
        {
            if (bandit.time_looted >= bandit.max_looting_time)
            {
                SetCurrentState("attack");
            }
        }

        current_state.StateUpdate();
    }

    public override void DoFixedUpdate()
    {
        FollowTarget();

        if (current_state==null) {return;}
        current_state.StateFixedUpdate();

        
    }

    void FollowTarget()
    {
        if (bandit == null || bandit.target == null) return;

        // Prefer NavMeshAgent when on a navmesh.
        if (bandit.agent != null && bandit.agent.enabled && bandit.agent.isOnNavMesh)
        {
            bandit.agent.destination = bandit.target.position;
            return;
        }

        // Fallback: velocity-based movement (used by train-deck spawns where
        // the spawner disables the agent because the NavMesh is on the ground
        // far below the train).
        Rigidbody rb = parent.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic) return;

        // Don't override horizontal velocity while airborne — otherwise bandits
        // slide sideways through the air after spawning above the deck.
        if (Mathf.Abs(rb.linearVelocity.y) > 0.5f) return;

        Vector3 toTarget = bandit.target.position - parent.transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        const float arriveDist = 0.5f;
        const float moveSpeed = 3.5f;

        Vector3 vel = rb.linearVelocity;
        if (dist > arriveDist)
        {
            Vector3 dir = toTarget / dist;
            vel.x = dir.x * moveSpeed;
            vel.z = dir.z * moveSpeed;
            // Face the target (flat).
            parent.transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            vel.x = 0f;
            vel.z = 0f;
        }
        rb.linearVelocity = vel;
    }

    void OnDrawGizmos()
    {   
        const float marker_size = .5f;
        if (bandit.target!=null)
        {
            // Draw Where Target is
            Gizmos.color = Color.violetRed;
            Gizmos.DrawCube(bandit.target.position,marker_size * new Vector3(1,1,1));
        }

        if (current_state!=null)
        {
            current_state.OnDrawGizmos();
        }
    }
}
