using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected BanditFSM fsm = null;

    protected Bandit bandit;

    protected GameObject parent;

    public void Init(Bandit bandit, BanditFSM fsm, GameObject parent) {
        this.fsm = fsm;
        this.parent = parent;
        this.bandit = fsm.bandit;
    }
    public virtual void StateEnter()
    {

    }
    public virtual void StateUpdate() {}
    public virtual void StateFixedUpdate() {}
    public virtual void StateExit() {}

    public virtual void OnDrawGizmos() {}

}
