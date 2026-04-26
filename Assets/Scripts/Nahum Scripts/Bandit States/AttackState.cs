using UnityEngine;

public class AttackState : State
{
    Vector3 target_pos;
    // GameObject[] barricades;

    Ray target_ray = new();
    Ray aim_ray = new();

    Ray direct_ray = new();

    float error_margin = 1f;

    float ray_distance = 20f;

    // Stop and shoot when within this distance of the player AND has LOS;
    // otherwise walk closer.
    const float engagement_range = 12f;

    bool reloading = false;


    public void Start()
    {
        
    }

    public override void StateEnter()
    {
        base.StateEnter();
        InvokeRepeating("TryShoot",0f,bandit.shoot_interval);
        Debug.Log("entered attack state.");
        SetTargetRay();
    }
    

    public override void StateExit()
    {
        base.StateExit();
        CancelInvoke("TryShoot");
        Debug.Log("exitted attack state.");
    }

    public override void StateUpdate()
    {
        if (bandit.mag_size==0 && !reloading)
        {
            print("reloading...");
            reloading = true;
            Invoke("Reload",bandit.reload_time);
        }

        // Always face the player while attacking — was only rotating when LOS was
        // blocked, so bandits kept facing the chest they walked from.
        if (bandit.enemy_target != null) LookToTargetEnemy();

        if (bandit.enemy_target == null)
        {
            bandit.target = parent.transform;
            return;
        }

        // In range AND has LOS → stop and shoot. Otherwise close the gap.
        float dist = Vector3.Distance(parent.transform.position, bandit.enemy_target.transform.position);
        bool inRange = dist <= engagement_range;
        if (inRange && CanSeeEnemyTarget())
        {
            bandit.target = parent.transform;
        }
        else
        {
            bandit.target = bandit.enemy_target.transform;
        }
    }

   

    public bool CanSeeEnemyTarget()
    {
        if (bandit.enemy_target==null) {return false;}
        // Raycast from chest height — feet-level origin was hitting the bandit's
        // own capsule or the train deck before the player.
        Vector3 origin = parent.transform.position + Vector3.up * 1f;
        Vector3 dir = bandit.enemy_target.transform.position - origin;
        RaycastHit hit;
        if (Physics.Raycast(origin, dir.normalized, out hit, dir.magnitude + 1f))
        {
            GameObject obj = hit.collider.gameObject;
            // Match the player root or any of its children (capsule on root, mesh as child).
            return obj == bandit.enemy_target || obj.transform.IsChildOf(bandit.enemy_target.transform);
        }
        return false;
    }

    public override void StateFixedUpdate()
    {
        aim_ray.origin = parent.transform.position;
        aim_ray.direction = parent.transform.forward;

        
        
        if (!IsAimedNearTarget())
        {
            SetTargetRay();
        }

        if (target_ray.direction.magnitude==0f)
        {
            target_ray.origin = aim_ray.origin;
            target_ray.direction = aim_ray.direction;
        }
    }

    public void SetTargetRay()
    {
        if (bandit.enemy_target)
        {
            // Chest-height origin so the LOS raycast doesn't self-hit the bandit's feet.
            Vector3 origin = parent.transform.position + Vector3.up * 1f;
            direct_ray.origin = origin;
            direct_ray.direction = bandit.enemy_target.transform.position - direct_ray.origin;

            target_ray.origin = origin;
            target_ray.direction = GetAimedAtPosition() - target_ray.origin;
        }
    }

    public void Reload()
    {
        bandit.mag_size = bandit.max_mag_size;
        print("reloading done.");
        reloading = false;
    }

    public bool IsAimedNearTarget()
    {
        // Widened from 12° to 20° so bandits don't sit silent for long when accuracy
        // randomness puts shots just outside the previous threshold.
        return !(Vector3.Angle(target_ray.direction,direct_ray.direction) > 20f);
    }

    public void TryShoot()
    {
        if (IsAimedNearTarget())
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (bandit.mag_size<=0) {return;}
        if (bandit.bullet_prefab == null) { Debug.LogWarning($"[{parent.name}] bullet_prefab not assigned"); return; }
        print("shot");
        // Spawn bullet well clear of the bandit's own capsule.
        Vector3 spawnPos = parent.transform.position + Vector3.up * 1f + parent.transform.forward * 1f;
        GameObject bullet = Instantiate(bandit.bullet_prefab, spawnPos, parent.transform.rotation);
        bullet.tag = "EnemyBullet";

        // Make sure the bullet has the damage handler — the prefab is just a
        // mesh+collider+rigidbody by default, no script.
        if (bullet.GetComponent<Bullet>() == null)
            bullet.AddComponent<Bullet>();

        // Ignore collision between the bullet and the firing bandit's capsule
        // so bullet doesn't self-destruct on spawn.
        Collider bulletCol = bullet.GetComponent<Collider>();
        Collider banditCol = parent.GetComponent<Collider>();
        if (bulletCol != null && banditCol != null)
            Physics.IgnoreCollision(bulletCol, banditCol);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = target_ray.direction.normalized * 15f; // direct velocity, predictable speed
        }
        bandit.mag_size = Mathf.Clamp(bandit.mag_size-1,0,bandit.max_mag_size);
        SetTargetRay();

        Destroy(bullet, 2f);
    }

    public void LookToTargetEnemy()
    {
        if (bandit.enemy_target == null) return;
        // Use a freshly-computed direction, not the (potentially stale) direct_ray.
        Vector3 dir = bandit.enemy_target.transform.position - parent.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion direct_rotation = Quaternion.LookRotation(dir.normalized);
        parent.transform.rotation = Quaternion.Slerp(parent.transform.rotation, direct_rotation, 5f * Time.deltaTime);
    }

    public Vector3 GetAimedAtPosition()
    {
        Vector3 epos = bandit.enemy_target.transform.position;
        float em = error_margin;
        Renderer rend = bandit.enemy_target.GetComponentInChildren<Renderer>();
        Vector3 esize = rend != null ? rend.bounds.size : Vector3.one;
        
        // attack radius values
        float rf = 1.4f;
        float arx = esize.x * rf;
        float ary = esize.y * rf;
        float arz = esize.z * rf;

        float rx = Random.Range(0f,1f) < bandit.accuracy ? Random.Range(epos.x - arx/2, epos.x + arx/2) 
            : epos.x + ((Random.Range(0f,1f) < .5f? 1 : -1) * Random.Range( arx, arx + em));
        float ry = Random.Range(epos.y - ary/2, epos.y + ary/2) ;
        float rz = Random.Range(0,1) < bandit.accuracy ? Random.Range(epos.z - arz/2, epos.z+arz/2) 
            : epos.z + (Random.Range(0f,1f) < .5f? 1 : -1) * Random.Range(arz, arz + em);

        return new (rx,ry,rz);
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(target_ray.origin,target_ray.direction * ray_distance);

        Gizmos.color = Color.ghostWhite;
        Gizmos.DrawRay(aim_ray.origin,aim_ray.direction * 7);

        Gizmos.color = Color.purple;
        Gizmos.DrawRay(direct_ray.origin,direct_ray.direction * 7);

        // if (fsm.enemy_target)
        // {
        //     Gizmos.color = Color.plum;

        //     Vector3 origin = parent.transform.position;
        //     target_pos = fsm.enemy_target.transform.position;
        //     Vector3 dir = target_pos - origin;
        //     Gizmos.DrawRay(origin,dir * 30);
        // }
    }
}
