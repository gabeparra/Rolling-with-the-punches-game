using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    //use commented version to use project-wide input mapping (doesn't currently work)
    private void OnEnable()
    {
        input.actions["Interact"].performed += OnInteract;
        input.actions["Interact"].Enable();
        // InputSystem.actions["Interact"].performed += OnInteract;
        // InputSystem.actions["Interact"].Enable();

    }

    //use commented version to use project-wide input mapping (doesn't currently work)
    private void OnDisable()
    {
        input.actions["Interact"].performed -= OnInteract;
        input.actions["Interact"].Disable();
        // InputSystem.actions["Interact"].performed -= OnInteract;
        // InputSystem.actions["Interact"].Disable();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("interact pressed");
    }
}
