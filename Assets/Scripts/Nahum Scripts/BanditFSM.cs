using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BanditFSM : FSM
{
    
    public Vector3 target;
    public GameObject floor;
  
    SearchState searchState;

    public GameObject loot_targets_container;
    private Transform[] loot_targets;

    public GameObject bullet_prefab;

    public GameObject enemy_target;

    NavMeshAgent agent;

    protected override void SetCurrentState(String state)
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

        states = new()
        {
        {"loot", parent.AddComponent<LootState>()},
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

        print(loot_targets);

        SetCurrentState("attack");
    }

    protected override void Update()
    {
        searchState.floor = floor;

        if (current_state==null) {return;}
        current_state.StateUpdate();
        
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
