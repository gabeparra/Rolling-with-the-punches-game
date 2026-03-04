using UnityEngine;
using System.Collections; // Required for Coroutines

public class StopSubanimations : MonoBehaviour
{
    public void StopAllSubAnimations()
    {
        // Start the Coroutine process
        StartCoroutine(PauseAndResumeRoutine());
    }

    IEnumerator PauseAndResumeRoutine()
    {
        // 1. Find and Disable all child animators
        Animator[] childAnimators = GetComponentsInChildren<Animator>();
        foreach (Animator anim in childAnimators)
        {
            anim.enabled = false;
        }

        Debug.Log("Sub-animations paused...");

        // 2. Wait for 10 seconds
        yield return new WaitForSeconds(10f);

        // 3. Re-enable all child animators
        foreach (Animator anim in childAnimators)
        {
            anim.enabled = true;
        }

        Debug.Log("Sub-animations resumed!");
    }
}
