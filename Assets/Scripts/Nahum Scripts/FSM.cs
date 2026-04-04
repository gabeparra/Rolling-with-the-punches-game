using System;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public GameObject parent;

    public Dictionary<string, State> states = new();
    public State current_state;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        parent = transform.gameObject;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        
    }

    protected virtual void ExitCurrentState()
    {
        if (current_state != null)
        {
            current_state.StateExit();
            current_state = null;
        }
    }

    public virtual void SetCurrentState(String state)
    {
        ExitCurrentState();
        current_state = states[state];
        current_state.StateEnter();
    }
}
