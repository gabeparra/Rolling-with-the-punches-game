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
        input.actions["Save"].performed += OnSave;
        input.actions["Save"].Enable();

        input.actions["Load"].performed += OnLoad;
        input.actions["Load"].Enable();

        // InputSystem.actions["Interact"].performed += OnInteract;
        // InputSystem.actions["Interact"].Enable();

    }

    //use commented version to use project-wide input mapping (doesn't currently work)
    private void OnDisable()
    {
        input.actions["Save"].performed -= OnSave;
        input.actions["Save"].Disable();

        input.actions["Load"].performed -= OnLoad;
        input.actions["Load"].Disable();

        // InputSystem.actions["Interact"].performed -= OnInteract;
        // InputSystem.actions["Interact"].Disable();
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
}
