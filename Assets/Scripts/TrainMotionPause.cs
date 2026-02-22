using UnityEngine;

public class TrainMotionPause : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PauseAndStartIllusion()
    {
        // 1. Pause the animator
        GetComponent<Animator>().speed = 0; 

        // 3. Resume after x seconds
        Invoke("ResumeAnimation", 10f);
    }

    void ResumeAnimation()
    {
        GetComponent<Animator>().speed = 1;
    }
}

