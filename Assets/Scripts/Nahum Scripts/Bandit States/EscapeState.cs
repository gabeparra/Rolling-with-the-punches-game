using UnityEngine;

public class EscapeState : State
{
    float fleeTimer;
    const float fleeDuration = 5f;
    const float fleeDistance = 10f;
    const float recalcDistance = 4f;

    public override void StateEnter()
    {
        fleeTimer = 0f;
        SetFleeTarget();
    }

    public override void StateUpdate()
    {
        fleeTimer += Time.deltaTime;

        // Recalculate if player gets too close
        if (fsm.enemy_target != null)
        {
            float dist = Vector3.Distance(parent.transform.position, fsm.enemy_target.transform.position);
            if (dist < recalcDistance)
                SetFleeTarget();
        }

        // Done fleeing — go to search
        if (fleeTimer >= fleeDuration)
            fsm.SetCurrentState("search");
    }

    public override void StateFixedUpdate()
    {
    }

    public override void StateExit()
    {
        CancelInvoke();
    }

    void SetFleeTarget()
    {
        Vector3 fleeDir;
        if (fsm.enemy_target != null)
        {
            fleeDir = (parent.transform.position - fsm.enemy_target.transform.position).normalized;
            fleeDir.y = 0f;
            if (fleeDir.sqrMagnitude < 0.01f)
                fleeDir = parent.transform.forward;
        }
        else
        {
            fleeDir = parent.transform.forward;
        }

        fsm.target = parent.transform.position + fleeDir * fleeDistance;
    }
}
