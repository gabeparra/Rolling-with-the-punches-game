using UnityEngine;

public class SmartSearchState : State
{
    public GameObject floor;

    ImprovedBanditFSM improvedFsm;

    public override void StateEnter()
    {
        improvedFsm = (ImprovedBanditFSM)fsm;
        InvokeRepeating("Search", 0f, 4f);
    }

    public override void StateExit()
    {
        CancelInvoke("Search");
    }

    public override void StateUpdate()
    {
        // Detect player while searching
        float dist = improvedFsm.DistanceToPlayer();
        if (dist <= improvedFsm.detectionRange && improvedFsm.CanSeePlayer())
        {
            fsm.SetCurrentState("attack");
            return;
        }
    }

    public override void StateFixedUpdate()
    {
    }

    void Search()
    {
        if (floor != null)
        {
            Vector3 pos = floor.transform.position;
            Vector3 bounds = floor.GetComponent<Renderer>().bounds.size;

            float rx = Random.Range(-bounds.x / 2, bounds.x / 2);
            float rz = Random.Range(-bounds.z / 2, bounds.z / 2);

            fsm.target = new Vector3(pos.x + rx, pos.y + bounds.y / 2 + 0.1f, pos.z + rz);
        }
        else
        {
            // No floor reference (e.g. spawned on train) — wander near current position.
            // If the player is assigned, head toward them instead of standing still.
            if (fsm.enemy_target != null)
            {
                fsm.SetCurrentState("attack");
                return;
            }
            float rx = Random.Range(-5f, 5f);
            float rz = Random.Range(-5f, 5f);
            Vector3 pos = parent.transform.position;
            fsm.target = new Vector3(pos.x + rx, pos.y, pos.z + rz);
        }
    }
}
