using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BanditFSM : FSM
{
    public int health = 6; // Added by Hector to set enemy health value
    public Vector3 target;
    public GameObject floor;
  
    SearchState searchState;

    LootState lootState;

    public GameObject loot_targets_container;
    public Transform[] loot_targets;

    public int time_looting = 0;
    public int time_looting_threshold = 5;

    public GameObject bullet_prefab;

    public GameObject enemy_target;

    NavMeshAgent agent;

    public override void SetCurrentState(String state)
    {
        base.SetCurrentState(state);
        //current_state.SetTarget(target);
    }

    protected override void Start()
    {
        base.Start();
        agent = parent.GetComponent<NavMeshAgent>();
        target = parent.transform.position;


        searchState = parent.AddComponent<SearchState>();
        lootState = parent.AddComponent<LootState>();


        states = new()
        {
        {"loot", lootState},
        {"escape", parent.AddComponent<EscapeState>()},
        {"attack", parent.AddComponent<AttackState>()},
        {"search", searchState}
        };

        foreach (State state in states.Values)
        {
            state.Init(this, parent);
        }

        if (loot_targets_container)
        {
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();
        }

        //print(loot_targets);

        //SetCurrentState("attack");
    }

    public void TakeDamage(int amount) // Method added by Hector to calculate damage taken by enemy
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die() // Method added by Hector to destroy enemy on death
    {
        Destroy(parent);
    }

    protected override void Update()
    {
        searchState.floor = floor;

        if (current_state==null) {return;}
        current_state.StateUpdate();

        if (current_state==lootState)
        {
            if (time_looting >= time_looting_threshold)
            {
                SetCurrentState("attack");
            }
        }
    }

    protected override void FixedUpdate()
    {
        FollowTarget();

        if (current_state==null) {return;}
        current_state.StateFixedUpdate();

        
    }

    void FollowTarget()
    {
        if (target!=parent.transform.position)
        {
            agent.destination = target;
        }
    }

    void OnDrawGizmos()
    {   
        const float marker_size = .5f;
        if (target!=null)
        {
            // Draw Where Target is
            Gizmos.color = Color.violetRed;
            Gizmos.DrawCube(target,marker_size * (new Vector3(1,1,1)));
        }

        if (current_state!=null)
        {
            current_state.OnDrawGizmos();
        }
    }
}
