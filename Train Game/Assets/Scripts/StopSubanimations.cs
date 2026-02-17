using UnityEngine;

public class StopSubanimations : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StopAllSubAnimations()
    {
        // This finds every Animator attached to children and turns them off
        Animator[] childAnimators = GetComponentsInChildren<Animator>();
        foreach (Animator anim in childAnimators)
        {
            anim.enabled = false;
        }
    }
}
