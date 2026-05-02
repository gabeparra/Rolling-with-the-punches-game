using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTetrisBackground : MonoBehaviour {

    [SerializeField] private InventoryTetris inventoryTetris;

    private bool isBuilt = false;

    public void BuildBackground() {
        if (isBuilt) return;
        isBuilt = true;

        // Fallback to singleton if serialized reference is missing
        if (inventoryTetris == null)
            inventoryTetris = InventoryTetris.Instance;
        if (inventoryTetris == null || inventoryTetris.GetGrid() == null) return;

        // Create background
        Transform template = transform.Find("Template");
        if (template == null) return;
        template.gameObject.SetActive(false);

        for (int x = 0; x < inventoryTetris.GetGrid().GetWidth(); x++) {
            for (int y = 0; y < inventoryTetris.GetGrid().GetHeight(); y++) {
                Transform backgroundSingleTransform = Instantiate(template, transform);
                backgroundSingleTransform.gameObject.SetActive(true);
            }
        }

        GetComponent<GridLayoutGroup>().cellSize = new Vector2(inventoryTetris.GetGrid().GetCellSize(), inventoryTetris.GetGrid().GetCellSize());

        GetComponent<RectTransform>().sizeDelta = new Vector2(inventoryTetris.GetGrid().GetWidth(), inventoryTetris.GetGrid().GetHeight()) * inventoryTetris.GetGrid().GetCellSize();

        GetComponent<RectTransform>().anchoredPosition = inventoryTetris.GetComponent<RectTransform>().anchoredPosition;
    }

}