using UnityEngine;

public class SmartLootState : State
{
    int timeLooting = 0;

    ImprovedBanditFSM improvedFsm;

    public override void StateEnter()
    {
        improvedFsm = (ImprovedBanditFSM)fsm;
        timeLooting = 0;
        SetLootTarget();
        InvokeRepeating("TryLoot", 0f, 1f);
    }

    public override void StateExit()
    {
        CancelInvoke("TryLoot");
    }

    public override void StateUpdate()
    {
        // Detect player at close range while looting
        float dist = improvedFsm.DistanceToPlayer();
        if (dist <= improvedFsm.detectionRange * 0.5f && improvedFsm.CanSeePlayer())
        {
            fsm.SetCurrentState("attack");
            return;
        }

        // Loot timer → attack transition
        if (timeLooting >= fsm.time_looting_threshold)
        {
            fsm.SetCurrentState("attack");
            return;
        }
    }

    public override void StateFixedUpdate()
    {
    }

    void TryLoot()
    {
        if (timeLooting >= fsm.time_looting_threshold) return;
        if (fsm.loot_targets == null || fsm.loot_targets.Length == 0) return;

        bool hasLootTarget = false;
        for (int i = 0; i < fsm.loot_targets.Length; i++)
        {
            if (fsm.loot_targets[i] != null && fsm.loot_targets[i].position == fsm.target)
            {
                hasLootTarget = true;
                break;
            }
        }

        if (hasLootTarget && Vector3.Distance(fsm.target, parent.transform.position) < 1f)
        {
            timeLooting++;
        }
    }

    void SetLootTarget()
    {
        // No loot targets — go after the player instead
        if (fsm.loot_targets == null || fsm.loot_targets.Length == 0)
        {
            fsm.SetCurrentState("attack");
            return;
        }

        Transform closest = null;
        for (int i = 0; i < fsm.loot_targets.Length; i++)
        {
            Transform potential = fsm.loot_targets[i];
            if (potential == null) continue;
            if (closest == null ||
                Vector3.Distance(potential.position, parent.transform.position) <
                Vector3.Distance(closest.position, parent.transform.position))
            {
                closest = potential;
            }
        }

        fsm.target = closest != null ? closest.position : parent.transform.position;
    }
}
