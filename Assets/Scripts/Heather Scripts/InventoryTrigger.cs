using UnityEngine;

public class InventoryTrigger : MonoBehaviour {

    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    private bool playerInRange = false;

    private void Start() {
        inventoryUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Trigger hit by: " + other.gameObject.name);
        if (other.CompareTag("Player")) {
            playerInRange = true;
            InventoryTetris.Instance.InitializeInventory();
            inventoryUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerInRange = false;
            inventoryUI.SetActive(false);
        }
    }

    private void Update() {
        if (playerInRange && Input.GetKeyDown(closeKey)) {
            inventoryUI.SetActive(false);
            playerInRange = false;
        }
    }
}