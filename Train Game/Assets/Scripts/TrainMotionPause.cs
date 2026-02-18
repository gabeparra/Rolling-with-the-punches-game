using UnityEngine;

public class TrainMotionPause : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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

