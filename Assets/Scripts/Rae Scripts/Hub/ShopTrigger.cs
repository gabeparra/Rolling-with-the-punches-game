using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ShopUI))]
public class ShopTrigger : MonoBehaviour
{
    private ShopUI shopUI;
    
    private bool canInteract = false;

    public void Awake()
    {
        shopUI = GetComponent<ShopUI>();
    }

    public void OnEnable()
    {
        InputSystem.actions["Interact"].performed += OnInteract;
    }

    public void OnDisable()
    {
        InputSystem.actions["Interact"].performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if(canInteract) Interact();
    }

    private void Interact()
    {
        shopUI.Show();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            
            canInteract = true;
            HubManager.ShowPrompt("Open Shop");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            HubManager.HidePrompt();
        }
    }
}
