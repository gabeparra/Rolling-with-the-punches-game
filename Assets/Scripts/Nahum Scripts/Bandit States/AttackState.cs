using UnityEngine;

public class AttackState : State
{
    Vector3 target_pos;
    GameObject[] barricades;

    Ray target_ray = new();
    Ray aim_ray = new();

    Ray direct_ray = new();

    float error_margin = 1f;

    public float shoot_interval = 1.5f;

    public float shoot_force = 2000f;

    int mag_size = 6;

    bool reloading = false;

    public int max_mag_size = 6;

    public float reload_time = 2f;

    public float accuracy = .3f;

    float ray_distance = 20f;

    public Bandit bandit_data;


    public void Start()
    {
        InvokeRepeating("TryShoot",0f,shoot_interval);
    }

    public override void StateEnter()
    {
        Debug.Log("entered fight state.");
        SetTargetRay();
    }
    public override void StateUpdate()
    {
        if (mag_size==0 && !reloading)
        {
            print("reloading...");
            reloading = true;
            Invoke("Reload",reload_time);
        }

        if (!CanSeeEnemyTarget() && fsm.enemy_target!=null)
        {
            fsm.target = fsm.enemy_target.transform.position;
            LookToTargetEnemy();
        }
        else
        {
            fsm.target = parent.transform.position;
        }
    }

    public bool CanSeeEnemyTarget()
    {
        if (fsm.enemy_target==null) {return false;}
        RaycastHit hit;
        if (Physics.Raycast(direct_ray, out hit,float.PositiveInfinity))
        {
            GameObject obj = hit.collider.gameObject;
            
            bool canSeePlayer = obj.Equals(fsm.enemy_target);
            if (canSeePlayer)
            {
                return true;
            }
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
        if (fsm.enemy_target)
        {
            direct_ray.origin = parent.transform.position;
            direct_ray.direction = fsm.enemy_target.transform.position - direct_ray.origin;

            target_ray.origin = parent.transform.position;
            //print(Vector3.Angle(target_ray.direction,direct_ray.direction));
            
            target_ray.direction = GetAimedAtPosition() - target_ray.origin;

            
        }
    }

    public void Reload()
    {
        mag_size = max_mag_size;
        print("reloading done.");
        reloading = false;
    }

    public bool IsAimedNearTarget()
    {
        return !(Vector3.Angle(target_ray.direction,direct_ray.direction) > 12f);
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
        if (mag_size<=0) {return;}
        print("shot");
        GameObject bullet = Instantiate(fsm.bullet_prefab, parent.transform.position + parent.transform.forward * .5f, parent.transform.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(target_ray.direction * shoot_force);
        mag_size = Mathf.Clamp(mag_size-1,0,max_mag_size);
        SetTargetRay();

        Destroy(bullet, 2f);
    }

    public void LookToTargetEnemy()
    {
        float f = 5f;
        Quaternion direct_rotation = Quaternion.LookRotation(direct_ray.direction);
        parent.transform.rotation = Quaternion.Slerp(parent.transform.rotation, direct_rotation, f * Time.deltaTime);
    }

    public Vector3 GetAimedAtPosition()
    {
        Vector3 epos = fsm.enemy_target.transform.position;
        float em = error_margin;
        Renderer rend = fsm.enemy_target.GetComponentInChildren<Renderer>();
        Vector3 esize = rend != null ? rend.bounds.size : Vector3.one;
        
        // attack radius values
        float rf = 1.4f;
        float arx = esize.x * rf;
        float ary = esize.y * rf;
        float arz = esize.z * rf;

        float rx = Random.Range(0f,1f) < accuracy ? Random.Range(epos.x - arx/2, epos.x + arx/2) 
            : epos.x + ((Random.Range(0f,1f) < .5f? 1 : -1) * Random.Range( arx, arx + em));
        float ry = Random.Range(epos.y - ary/2, epos.y + ary/2) ;
        float rz = Random.Range(0,1) < accuracy ? Random.Range(epos.z - arz/2, epos.z+arz/2) 
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
