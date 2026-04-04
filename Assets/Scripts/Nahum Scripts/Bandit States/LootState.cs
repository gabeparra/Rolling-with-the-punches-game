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
        if (fsm.time_looting_threshold <= fsm.time_looting) {
        
            return;
        }
        bool has_loot_target = false;
        for (int i =0; i<fsm.loot_targets.Length; i++)
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
        Transform closest = null;

        if (fsm.loot_targets == null || fsm.loot_targets.Length==0) {
            fsm.target = parent.transform.position;
        }
        for (int i = 0; i < fsm.loot_targets.Length; i++)
        {
            Transform potential = fsm.loot_targets[i];
            if (!closest)
            {
                closest = potential;
            }
            if (Vector3.Distance(closest.position, transform.position) > Vector3.Distance(potential.position, transform.position))
            {
                closest = potential;
            }
            print(potential);
        }
        fsm.target = closest.position;
    }
}
