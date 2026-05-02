// using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LootState : State
{
    // float time_looting = 0f;

    public override void StateEnter()
    {
        Debug.Log("entered loot state.");
        bandit.time_looted = 0f;
        SetLootTarget();
        InvokeRepeating("TryLoot",0,1);
    }

    public override void StateExit()
    {
        base.StateExit();
        CancelInvoke("TryLoot");
        bandit.time_looted = 0f;
    }

    public override void StateUpdate()
    {
        
    }

    public override void StateFixedUpdate()
    {
        
    }

    public void TryLoot()
    {
        // if (GameManager.getCurrency()<=0)
        // {
        //     StateExit();
        // }
        if (bandit.loot_targets == null || bandit.loot_targets.Length == 0) return;

        bool has_loot_target = false;
        for (int i = 0; i < bandit.loot_targets.Length; i++)
        {
            Transform target = bandit.loot_targets[i];
            if (target==bandit.target)
            {
                has_loot_target = true;
                break;
            }
        }

        if (has_loot_target && Vector3.Distance(bandit.target.position,parent.transform.position) < 1.5f)
        {
            bandit.time_looted+=1;
            int currency = GameManager.getCurrency();
            int taking = Mathf.FloorToInt(currency * bandit.loot_take_percentage);
            // FloorToInt rounds 20%-of-small-numbers to zero, so gold never
            // reaches 0 and the gold-zero attack switch never fires. Mop up
            // the remainder when the percentage rounds to nothing.
            if (taking <= 0 && currency > 0) taking = currency;
            // HUDManager.LoseGold updates both the HUD and the GameManager save.
            if (HUDManager.Instance != null) HUDManager.Instance.LoseGold(taking);
            else GameManager.UpdateCurrency(-taking);
            print("time looted: " + bandit.time_looted);
            print("took: " + taking);
        }
        
    }

    public void SetLootTarget()
    {
        if (bandit.loot_targets == null || bandit.loot_targets.Length == 0)
        {
            bandit.target = bandit.transform;
            StateExit();
            return;
        }

        if (bandit.loot_follow_type == Bandit.LootFollowType.Random)
        {
            foreach (Transform targ in bandit.loot_targets)
            {
                print(targ.gameObject);
            }
            int ri = Random.Range(0,bandit.loot_targets.Length);
            bandit.target = bandit.loot_targets[ri];
            return;
        }

        Transform closest = null;
        for (int i = 0; i < bandit.loot_targets.Length; i++)
        {
            Transform potential = bandit.loot_targets[i];
            if (potential == null) continue;
            if (closest == null || Vector3.Distance(potential.position, parent.transform.position) < Vector3.Distance(closest.position, parent.transform.position))
                closest = potential;
        }

        bandit.target = closest != null ? closest : bandit.transform;
    }
}
