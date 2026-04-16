using UnityEngine;

public class SmartAttackState : State
{
    Ray targetRay = new();
    Ray aimRay = new();
    Ray directRay = new();

    float errorMargin = 1f;
    float accuracy = 0.3f;
    float rayDistance = 20f;
    float bulletSpeed = 8f;

    const int maxMagSize = 6;
    int magSize = maxMagSize;
    float shootInterval = 0.75f;
    bool reloading = false;
    float reloadTime = 2f;

    float timeSinceLastSeen = 0f;
    float disengageTime = 4f;

    ImprovedBanditFSM improvedFsm;
    LineRenderer laserLine;

    public override void StateEnter()
    {
        improvedFsm = (ImprovedBanditFSM)fsm;
        timeSinceLastSeen = 0f;
        magSize = maxMagSize;
        reloading = false;
        SetTargetRay();
        InvokeRepeating("TryShoot", 0f, shootInterval);

        // Create laser sight
        laserLine = parent.GetComponent<LineRenderer>();
        if (laserLine == null)
            laserLine = parent.AddComponent<LineRenderer>();
        laserLine.startWidth = 0.03f;
        laserLine.endWidth = 0.03f;
        laserLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        laserLine.material.color = Color.red;
        laserLine.startColor = Color.red;
        laserLine.endColor = Color.red;
        laserLine.positionCount = 2;
        laserLine.enabled = true;
    }

    public override void StateExit()
    {
        CancelInvoke("TryShoot");
        CancelInvoke("Reload");
        if (laserLine != null) laserLine.enabled = false;
    }

    public override void StateUpdate()
    {
        if (magSize == 0 && !reloading)
        {
            reloading = true;
            Invoke("Reload", reloadTime);
        }

        bool canSee = improvedFsm.CanSeePlayer();
        float dist = improvedFsm.DistanceToPlayer();
        bool inRange = dist <= improvedFsm.attackRange;

        if (canSee)
        {
            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }

        // Disengage if lost sight for too long
        if (timeSinceLastSeen > disengageTime)
        {
            fsm.SetCurrentState("search");
            return;
        }

        // Movement: chase if out of range or no LOS, hold if in range with LOS
        if (canSee && inRange)
        {
            fsm.target = parent.transform.position; // hold position
        }
        else if (fsm.enemy_target != null)
        {
            fsm.target = fsm.enemy_target.transform.position; // chase
        }

        // Always rotate toward player
        LookToTargetEnemy();

        // Update laser sight
        if (laserLine != null && laserLine.enabled && fsm.enemy_target != null)
        {
            Vector3 origin = parent.transform.position + parent.transform.forward * 0.5f;
            laserLine.SetPosition(0, origin);
            laserLine.SetPosition(1, fsm.enemy_target.transform.position);
        }
    }

    public override void StateFixedUpdate()
    {
        aimRay.origin = parent.transform.position;
        aimRay.direction = parent.transform.forward;

        if (!IsAimedNearTarget())
        {
            SetTargetRay();
        }

        if (targetRay.direction.magnitude == 0f)
        {
            targetRay.origin = aimRay.origin;
            targetRay.direction = aimRay.direction;
        }
    }

    void SetTargetRay()
    {
        if (fsm.enemy_target)
        {
            directRay.origin = parent.transform.position;
            directRay.direction = fsm.enemy_target.transform.position - directRay.origin;

            targetRay.origin = parent.transform.position;
            targetRay.direction = GetAimedAtPosition() - targetRay.origin;
        }
    }

    void Reload()
    {
        magSize = maxMagSize;
        reloading = false;
    }

    bool IsAimedNearTarget()
    {
        return Vector3.Angle(targetRay.direction, directRay.direction) <= 12f;
    }

    void TryShoot()
    {
        if (fsm.current_state != this) return; // only shoot while in attack state
        if (IsAimedNearTarget())
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (magSize <= 0) return;
        if (fsm.bullet_prefab == null) return;

        GameObject bullet = Object.Instantiate(
            fsm.bullet_prefab,
            parent.transform.position + parent.transform.forward * 1.5f,
            parent.transform.rotation
        );
        bullet.tag = "EnemyBullet";

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = targetRay.direction.normalized * bulletSpeed;
        }

        magSize = Mathf.Clamp(magSize - 1, 0, maxMagSize);
        SetTargetRay();
        Object.Destroy(bullet, 4f);
    }

    void LookToTargetEnemy()
    {
        if (fsm.enemy_target == null) return;
        Vector3 dir = fsm.enemy_target.transform.position - parent.transform.position;
        dir.y = 0;
        if (dir == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        parent.transform.rotation = Quaternion.Slerp(
            parent.transform.rotation, targetRotation, 5f * Time.deltaTime
        );
    }

    Vector3 GetAimedAtPosition()
    {
        Vector3 epos = fsm.enemy_target.transform.position;
        Renderer rend = fsm.enemy_target.GetComponentInChildren<Renderer>();
        Vector3 esize = rend != null ? rend.bounds.size : Vector3.one;

        float rf = 1.4f;
        float arx = esize.x * rf;
        float ary = esize.y * rf;
        float arz = esize.z * rf;

        float rx = Random.Range(0f, 1f) < accuracy
            ? Random.Range(epos.x - arx / 2, epos.x + arx / 2)
            : epos.x + ((Random.Range(0f, 1f) < 0.5f ? 1 : -1) * Random.Range(arx, arx + errorMargin));
        float ry = Random.Range(epos.y - ary / 2, epos.y + ary / 2);
        float rz = Random.Range(0f, 1f) < accuracy
            ? Random.Range(epos.z - arz / 2, epos.z + arz / 2)
            : epos.z + ((Random.Range(0f, 1f) < 0.5f ? 1 : -1) * Random.Range(arz, arz + errorMargin));

        return new Vector3(rx, ry, rz);
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(targetRay.origin, targetRay.direction * rayDistance);

        Gizmos.color = Color.white;
        Gizmos.DrawRay(aimRay.origin, aimRay.direction * 7);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(directRay.origin, directRay.direction * 7);
    }
}
