using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 4;
    private Vector3 movementVector = Vector3.zero;

    //the default input mapping suddenly started working again... keep the temporary stuff available in case it breaks again
    //private PlayerInput input;
    public static bool canMove = true; //set to false to prevent movement (like when in a shop)

    private void Awake()
    {
        //input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    //use input.actions instead of InputSystem.actions if default action mapping stops working again
    private void OnEnable()
    {
        //subscribe to both press and release of movement
        InputSystem.actions["Move"].performed += OnMove;
        InputSystem.actions["Move"].canceled += OnMove;
        InputSystem.actions["Move"].Enable();

        InputSystem.actions["Save"].performed += OnSave;
        InputSystem.actions["Save"].Enable();

        InputSystem.actions["Load"].performed += OnLoad;
        InputSystem.actions["Load"].Enable();

    }

    //use input.actions instead of InputSystem.actions if default action mapping stops working again
    private void OnDisable()
    {
        //unsubscribe from both press and release of movement
        InputSystem.actions["Move"].performed -= OnMove;
        InputSystem.actions["Move"].canceled -= OnMove;
        InputSystem.actions["Move"].Disable();

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

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if(!canMove)
        {
            movementVector = Vector3.zero;
            return;
        }

        movementVector = InputSystem.actions["Move"].ReadValue<Vector2>();
        movementVector = new Vector3(movementVector.x, movementVector.z, movementVector.y);
    }

    
    private void FixedUpdate()
    {
        rb.linearVelocity = movementVector * speed;
    }

}
