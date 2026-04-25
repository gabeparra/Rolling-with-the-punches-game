using Unity.VisualScripting;
using UnityEngine;

public class SearchState : State
{
    public GameObject floor;

    bool at_target = false;

    public void Start()
    {
        at_target = bandit.target==parent.transform;
        
    }

    public override void StateEnter()
    {
        Debug.Log("entered search state.");
        InvokeRepeating("Search",0.0f,4f);
    }

    public override void StateExit()
    {
        print("stopped search");
        CancelInvoke("Search");
    }

    public override void StateUpdate()
    {
        //Debug.Log("floor: " + floor);
    }

    void Search()
    {
        print("searching for new point.");
        if (floor != null //&& at_target
        )
        {
            print("new");
            Vector3 pos = floor.transform.position;
            Vector3 bounds = floor.GetComponent<Renderer>().bounds.size;
            float xl = bounds[0];
            float yl = bounds[1];
            float zl = bounds[2];

            
            float rx = Random.Range(-xl/2,xl/2);
            float rz = Random.Range(-zl/2,zl/2);

            float x = pos[0] + rx;
            float y = pos[1] + yl/2 + .1f;
            float z = pos[2] + rz;

            bandit.target.position = new Vector3(x,y,z);
        }
    }

    public override void StateFixedUpdate()
    {
        at_target = bandit.target==parent.transform;
    }

    
}
