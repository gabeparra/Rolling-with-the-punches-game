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

        states = new()
        {
        {"loot", lootState},
        {"attack", attackState},
        {"idle", idleState}
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

        // Revert to attack after no more good can be taken
        if (current_state==lootState)
        {
            if (GameManager.getCurrency()<=0)
            {
                // SetCurrentState("attack");
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
        bandit.agent.destination = bandit.target.position;
        // print(bandit.target.gameObject);
        // print(bandit.target.position);
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
