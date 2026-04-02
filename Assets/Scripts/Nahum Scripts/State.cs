using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected BanditFSM fsm = null;
    protected GameObject parent;

    public void Init(BanditFSM fsm, GameObject parent) {
        this.fsm = fsm;
        this.parent = parent;
    }
    public virtual void StateEnter()
    {

    }
    public virtual void StateUpdate() {}
    public virtual void StateFixedUpdate() {}
    public virtual void StateExit() {}

    public virtual void OnDrawGizmos() {}

}
