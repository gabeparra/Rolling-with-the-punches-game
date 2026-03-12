using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    //the default input mapping suddenly started working again... keep the temporary stuff available in case it breaks again
    //private PlayerInput input;
    public static bool canMove = true; //set to false to prevent movement (like when in a shop)

    private void Awake()
    {
        //input = GetComponent<PlayerInput>();
    }

    //use input.actions instead of InputSystem.actions if default action mapping stops working again
    private void OnEnable()
    {
        InputSystem.actions["Save"].performed += OnSave;
        InputSystem.actions["Save"].Enable();

        InputSystem.actions["Load"].performed += OnLoad;
        InputSystem.actions["Load"].Enable();

    }

    //use input.actions instead of InputSystem.actions if default action mapping stops working again
    private void OnDisable()
    {
        InputSystem.actions["Save"].performed -= OnSave;
        InputSystem.actions["Save"].Disable();

        InputSystem.actions["Load"].performed -= OnLoad;
        InputSystem.actions["Load"].Disable();
    }

    private void OnSave(InputAction.CallbackContext ctx)
    {
        Debug.Log("save");
        GameManager.Instance.Save();
    }

    private void OnLoad(InputAction.CallbackContext ctx)
    {
        Debug.Log("load");
        GameManager.Instance.Load();
        
    }

    private float speed = 4;
    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        int up = Keyboard.current.wKey.isPressed ? 1 : 0;
        int left = Keyboard.current.aKey.isPressed ? 1 : 0;
        int down = Keyboard.current.sKey.isPressed ? 1 : 0;
        int right = Keyboard.current.dKey.isPressed ? 1 : 0;

        rb.linearVelocity = canMove ? new Vector3((right - left),0,(up-down)) * speed : Vector3.zero;
    }

}
