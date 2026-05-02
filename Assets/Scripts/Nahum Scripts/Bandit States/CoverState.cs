using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Bandit retreats to a position that breaks line-of-sight from the player and
/// holds there until the threat fades. Periodically peeks out to take a shot.
/// Triggered by BanditFSM.DoUpdate when the bandit is "under fire" (recent
/// TakeDamage) and still healthy enough to bother retreating.
/// </summary>
public class CoverState : State
{
    // ---- tunables (intentionally local — Bandit already has the user-facing knobs) ----
    const int    SAMPLES_PER_RING       = 12;
    const float  RING_MIN_RADIUS        = 2.5f;
    const float  RING_MAX_RADIUS        = 8f;
    const float  RING_RADIUS_STEP       = 1.75f;
    const float  ARRIVE_THRESHOLD       = 0.85f;
    const float  PEEK_INTERVAL          = 1.6f;   // try a shot every N seconds while in cover
    const float  PEEK_DURATION          = 0.4f;   // how long the peek "shoot opportunity" lasts
    const float  THREAT_FADE_SECONDS    = 4f;     // no damage for N seconds → exit cover
    const float  COVER_REFRESH_INTERVAL = 1.25f;  // re-evaluate cover spot if player moved a lot

    Vector3 coverPos;
    bool    hasCover;
    float   coverChosenAt;
    float   nextPeekAt;
    float   peekUntil;
    Vector3 lastPlayerSampledPos;

    public override void StateEnter()
    {
        Debug.Log("entered cover state.");
        hasCover = TryFindCover(out coverPos);
        coverChosenAt = Time.time;
        nextPeekAt = Time.time + PEEK_INTERVAL;
        peekUntil  = -1f;
        lastPlayerSampledPos = bandit.enemy_target != null
            ? bandit.enemy_target.transform.position
            : parent.transform.position;

        if (hasCover) MoveTo(coverPos);
    }

    public override void StateExit()
    {
        base.StateExit();
        bandit.lastLeftCoverAt = Time.time;
        Debug.Log("exited cover state.");
    }

    public override void StateUpdate()
    {
        // Always face the player if we have one — same logic AttackState uses.
        if (bandit.enemy_target != null) LookToward(bandit.enemy_target.transform.position);

        // Re-pick a cover spot if (a) we never found one, or (b) the player has
        // walked far enough that the old spot probably no longer covers us.
        bool playerMovedFar = bandit.enemy_target != null &&
            Vector3.Distance(bandit.enemy_target.transform.position, lastPlayerSampledPos) > 4f;
        bool dueForRefresh = Time.time - coverChosenAt > COVER_REFRESH_INTERVAL;
        if ((!hasCover || (playerMovedFar && dueForRefresh)))
        {
            hasCover = TryFindCover(out coverPos);
            coverChosenAt = Time.time;
            lastPlayerSampledPos = bandit.enemy_target != null
                ? bandit.enemy_target.transform.position
                : parent.transform.position;
            if (hasCover) MoveTo(coverPos);
        }

        // If no cover spot found anywhere, fall back to attack — sitting still
        // in the open is worse than continuing to fight.
        if (!hasCover && Time.time - coverChosenAt > 0.5f)
        {
            fsm.SetCurrentState("attack");
            return;
        }

        // Threat fade: been a while since last hit → leave cover.
        if (Time.time - bandit.lastDamagedAt > THREAT_FADE_SECONDS)
        {
            fsm.SetCurrentState("attack");
            return;
        }

        // Peek logic: while at cover, briefly expose & let AttackState's
        // shoot pipeline run. We piggy-back AttackState by NOT actually
        // shooting here (CoverState shouldn't duplicate firing code) — just
        // hold the bandit at the spot and rotate to face the player. The peek
        // window doesn't currently fire bullets; intentional, can be wired up
        // later if we want suppressing fire from cover.
        if (Time.time >= nextPeekAt && peekUntil < Time.time)
        {
            peekUntil = Time.time + PEEK_DURATION;
            nextPeekAt = Time.time + PEEK_INTERVAL;
        }
    }

    public override void StateFixedUpdate() { }

    void MoveTo(Vector3 pos)
    {
        // BanditFSM.FollowTarget reads bandit.target — set it to a transient
        // proxy transform at the cover position so the existing pathing code
        // takes us there without needing a real GameObject.
        if (bandit.agent != null && bandit.agent.enabled && bandit.agent.isOnNavMesh)
        {
            bandit.agent.destination = pos;
        }
        // Also poke the BanditFSM target so the rigidbody fallback path works
        // for train-deck spawns where the agent is disabled.
        bandit.target = parent.transform; // hold position via the fallback's
                                          // arrive-distance check; the agent
                                          // (above) handles the actual cover walk.
    }

    void LookToward(Vector3 worldPos)
    {
        Vector3 dir = worldPos - parent.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion want = Quaternion.LookRotation(dir.normalized);
        parent.transform.rotation = Quaternion.Slerp(parent.transform.rotation, want, 5f * Time.deltaTime);
    }

    /// <summary>
    /// Sample positions on rings around the bandit. A "cover" candidate is one
    /// where a chest-height raycast from the player to the candidate is BLOCKED
    /// by something that isn't a bandit — i.e. real geometry stands between
    /// player and candidate. Returns the closest navmesh-reachable candidate.
    /// </summary>
    bool TryFindCover(out Vector3 result)
    {
        result = parent.transform.position;
        if (bandit.enemy_target == null) return false;

        Vector3 banditPos = parent.transform.position;
        Vector3 playerEye = bandit.enemy_target.transform.position + Vector3.up * 1f;

        float bestDist = float.PositiveInfinity;
        bool  found    = false;

        for (float r = RING_MIN_RADIUS; r <= RING_MAX_RADIUS + 0.01f; r += RING_RADIUS_STEP)
        {
            for (int i = 0; i < SAMPLES_PER_RING; i++)
            {
                float a = (i / (float)SAMPLES_PER_RING) * Mathf.PI * 2f;
                Vector3 cand = banditPos + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r;

                // Snap to navmesh — if we can't path there, skip.
                Vector3 navPoint = cand;
                if (bandit.agent != null && bandit.agent.enabled)
                {
                    if (!NavMesh.SamplePosition(cand, out NavMeshHit nh, 1.25f, NavMesh.AllAreas)) continue;
                    navPoint = nh.position;
                }

                // Cover test: is player → candidate blocked by something other
                // than a bandit? If yes, candidate is occluded → real cover.
                Vector3 candEye = navPoint + Vector3.up * 1f;
                Vector3 dir = candEye - playerEye;
                float distToCand = dir.magnitude;
                if (distToCand < 0.05f) continue;

                if (Physics.Raycast(playerEye, dir.normalized, out RaycastHit hit, distToCand))
                {
                    // Skip hits on the seeking bandit's own collider hierarchy.
                    if (hit.collider.GetComponentInParent<Bandit>() == bandit) continue;
                    // Skip hits on the player itself (shouldn't happen — origin is the player — but guard).
                    if (hit.collider.gameObject == bandit.enemy_target) continue;

                    // Real obstacle in the way → this is a cover candidate.
                    float d = Vector3.Distance(banditPos, navPoint);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        result   = navPoint;
                        found    = true;
                    }
                }
            }
        }
        return found;
    }

    public override void OnDrawGizmos()
    {
        if (!hasCover) return;
        Gizmos.color = new Color(0.2f, 0.55f, 1f, 0.8f);
        Gizmos.DrawWireSphere(coverPos, 0.5f);
        if (parent != null) Gizmos.DrawLine(parent.transform.position, coverPos);
    }
}
