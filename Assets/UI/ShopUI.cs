using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField]
    private List<Upgrade> upgrades = new();

    private UIDocument uiDocument;
    private Button _btnClose;
    private ListView _list;
    private Label _goldLabel;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        _btnClose = uiDocument.rootVisualElement.Q("close") as Button;
        _list = uiDocument.rootVisualElement.Q("listView") as ListView;
        _goldLabel = uiDocument.rootVisualElement.Q<Label>("goldLabel");
        _list.itemsSource = upgrades;

        Hide();

        _list.bindItem = (e, i) =>
        {
            Upgrade u = upgrades[i];
            e.Q<Label>("name").text = u.upgradeName;
            e.Q<Label>("description").text = u.description;

            Button buyBtn = e.Q<Button>("btn");
            bool owned = GameManager.Instance != null && GameManager.Instance.HasUpgrade(u.id);
            if (owned)
            {
                buyBtn.text = "OWNED";
                buyBtn.SetEnabled(false);
            }
            else
            {
                buyBtn.text = u.cost.ToString() + "g";
                buyBtn.SetEnabled(true);
                buyBtn.clicked += () => TryBuy(u, buyBtn);
            }
        };
    }

    private void TryBuy(Upgrade u, Button buyBtn)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.HasUpgrade(u.id)) return;

        if (GameManager.Instance.BuyUpgrade(u.id, u.cost))
        {
            buyBtn.text = "OWNED";
            buyBtn.SetEnabled(false);
            if (_goldLabel != null)
                _goldLabel.text = "Gold: " + GameManager.Instance.Gold;
            Debug.Log("Purchased upgrade: " + u.upgradeName);
        }
        else
        {
            Debug.Log("Not enough gold for: " + u.upgradeName);
        }
    }

    public void Hide()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        PlayerController.canMove = true;
    }

    public void Show()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        PlayerController.canMove = false;
        // Update gold display
        if (_goldLabel != null && GameManager.Instance != null)
            _goldLabel.text = "Gold: " + GameManager.Instance.Gold;
        // Refresh list to update owned states
        if (_list != null) _list.Rebuild();
    }

    void OnEnable()
    {
        if (_btnClose != null)
            _btnClose.clicked += Hide;
    }

    void OnDisable()
    {
        if (_btnClose != null)
            _btnClose.clicked -= Hide;
    }
}
