using UnityEngine;
using UnityEngine.InputSystem;

// Original trigger by Rae, interact-to-open and exit handling added by Gabriel
[RequireComponent(typeof(ShopUI))]
public class ShopTrigger : MonoBehaviour
{
    private ShopUI shopUI;
    private bool playerInZone;

    public void Awake()
    {
        shopUI = GetComponent<ShopUI>();
    }

    void OnEnable()
    {
        InputSystem.actions["Interact"].performed += OnInteract;
    }

    void OnDisable()
    {
        InputSystem.actions["Interact"].performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (playerInZone)
            shopUI.Show();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            HubManager.ShowPrompt("Press E to open Shop");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            HubManager.HidePrompt();
            shopUI.Hide();
        }
    }
}
