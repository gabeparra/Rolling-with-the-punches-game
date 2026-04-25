using System.Linq;
using UnityEngine;

public class IdleState : State
{
    
    public override void StateEnter()
    {
        print("entered idle state.");
    }

    public override void StateExit()
    {
        base.StateExit();
        print("exited idle state.");
    }

    public override void StateUpdate()
    {
        
    }

    public override void StateFixedUpdate()
    {
        
    }

}



