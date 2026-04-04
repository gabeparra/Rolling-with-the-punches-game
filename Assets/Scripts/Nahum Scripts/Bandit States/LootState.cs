using System.Linq;
using UnityEngine;

public class LootState : State
{
    bool looting = false;

    public override void StateEnter()
    {
        Debug.Log("entered loot state.");
        SetLootTarget();
        InvokeRepeating("TryLoot",0,1);
    }

    public override void StateExit()
    {
        base.StateExit();
        looting = false;
        CancelInvoke("TryLoot");
    }

    public override void StateUpdate()
    {
        
    }

    public override void StateFixedUpdate()
    {
        
    }

    public void TryLoot()
    {
        if (fsm.time_looting_threshold <= fsm.time_looting) return;
        if (fsm.loot_targets == null || fsm.loot_targets.Length == 0) return;

        bool has_loot_target = false;
        for (int i = 0; i < fsm.loot_targets.Length; i++)
        {
            Vector3 pos = fsm.loot_targets[i].position;
            if (pos==fsm.target)
            {
                has_loot_target = true;
                break;
            }
        }
        if (has_loot_target && Vector3.Distance(fsm.target,parent.transform.position) < 1)
        {
            fsm.time_looting+=1;
        }
        print("looting time: " + fsm.time_looting);
    }

    public void SetLootTarget()
    {
        if (fsm.loot_targets == null || fsm.loot_targets.Length == 0)
        {
            fsm.target = parent.transform.position;
            return;
        }

        Transform closest = null;
        for (int i = 0; i < fsm.loot_targets.Length; i++)
        {
            Transform potential = fsm.loot_targets[i];
            if (potential == null) continue;
            if (closest == null || Vector3.Distance(potential.position, parent.transform.position) < Vector3.Distance(closest.position, parent.transform.position))
                closest = potential;
        }

        fsm.target = closest != null ? closest.position : parent.transform.position;
    }
}
