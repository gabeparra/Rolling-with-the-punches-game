using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy_AI : MonoBehaviour
{
    public GameObject head;
    public GameObject bulletPrefab;
    public GameObject loot_targets_container;
    private Transform target;
    private NavMeshAgent agent;
    private Transform[] loot_targets;
    public List<GameObject> enemy_list;
    public GameObject player;
    const float detection_range = 50f;
    public float shootInterval = .75f;
    Vector3 shootOrigin;
    Vector3 shootDirection;
    float shootForce = 80;
    bool canSeePlayer = false;
    

    public enum  State{
        LOOT,
        FIGHT
    }
    public State state = State.LOOT;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //state = Random.Range(0,2) ==0? State.LOOT : State.FIGHT;
        agent = GetComponent<NavMeshAgent>();
        if (loot_targets_container)
            loot_targets = loot_targets_container.GetComponentsInChildren<Transform>();
        InvokeRepeating("shoot",0,shootInterval);
        head = transform.Find("Head") ? transform.Find("Head").gameObject : null;

    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.collider.gameObject;
        if (obj)
        {
            Invoke("stopKnockback", 1f);
            Debug.Log("OWW");
        }
    }

    void stopKnockback()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }


    private void OnDrawGizmos()
    {
        //head = transform.Find("Head") ? transform.Find("Head").gameObject : null;
        shootOrigin = head.transform.position;
        shootDirection = head.transform.forward;
        Gizmos.color = Color.indianRed;
        Gizmos.DrawRay(shootOrigin, shootDirection * shootForce);
    }

    void shoot()
    {
        if (!canSeePlayer || state!=State.FIGHT) { return; }
        GameObject bullet = Instantiate(bulletPrefab, head.transform.position + head.transform.forward * 2, head.transform.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(head.transform.forward * shootForce);

        Destroy(bullet, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.LOOT)
        {
            doLootProtocol();
            if (agent.velocity.magnitude!=0)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(agent.velocity.normalized.x, 0, agent.velocity.normalized.z), Vector3.up);
            }
            
        }
        else if (state == State.FIGHT)
        {
            doFightProtocol();

        }

        //Rigidbody rb = GetComponent<Rigidbody>();
        if (target==null)
        {
            agent.SetDestination(transform.position);
        }
        
    }

    void doFightProtocol()
    {
        if (!player) { return; }
        target = player.transform;

        Ray ray = new Ray(transform.position, player.transform.position - transform.position);

       
        Debug.DrawRay(ray.origin, ray.direction * detection_range, Color.purple,0f);
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 0f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detection_range))
        {
            GameObject obj = hit.collider.gameObject;
            //Debug.Log(obj);
            //Debug.Log(player);

            canSeePlayer = obj.Equals(player);
            if (canSeePlayer)
            {
                //Debug.Log("I CAN SEEE!!!");
                target = null;
                transform.rotation = Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(ray.direction),1f);
            }
        }

        if (target!=null)
        { 
            agent.SetDestination(target.position);
        }

        
    }

    void doLootProtocol()
    {
        if (target == null)
        {
            Transform closest = null;
            if (loot_targets == null) return;
            for (int i = 0; i < loot_targets.Length; i++)
            {
                Transform potential = loot_targets[i];
                if (!closest)
                {
                    closest = potential;
                }
                if (Vector3.Distance(closest.position, transform.position) > Vector3.Distance(potential.position, transform.position))
                {
                    closest = potential;
                }
            }
            target = closest;
        }

        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void OnDestroy()
    {
        enemy_list.Remove(transform.parent.gameObject);
    }
}
