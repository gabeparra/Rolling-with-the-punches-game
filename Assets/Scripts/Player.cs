using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    float speed = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        int up = Keyboard.current.wKey.isPressed ? 1 : 0;
        int left = Keyboard.current.aKey.isPressed ? 1 : 0;
        int down = Keyboard.current.sKey.isPressed ? 1 : 0;
        int right = Keyboard.current.dKey.isPressed ? 1 : 0;

        rb.linearVelocity = new Vector3((right - left),0,(up-down)) * speed;
    }




}
