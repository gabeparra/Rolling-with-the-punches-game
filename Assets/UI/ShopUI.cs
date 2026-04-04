using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    //list of upgrades available in shop
    [SerializeField]
    private List<Upgrade> upgrades = new();

    private UIDocument uiDocument;
    private Button _btnClose;
    private ListView _list;


    void Awake()
    {
        //initialize references to UI objects
        uiDocument = GetComponent<UIDocument>();
        _btnClose = uiDocument.rootVisualElement.Q("close") as Button;
        _list = uiDocument.rootVisualElement.Q("listView") as ListView;
        _list.itemsSource = upgrades;

        Hide(); //hide shop UI until player actually goes to shop


        _list.bindItem = (e, i) =>
        {
            Upgrade u = upgrades[i];
            e.Q<Label>("name").text = u.upgradeName;
            e.Q<Label>("description").text = u.description;
            e.Q<Button>("btn").text = u.cost.ToString();
        };
    }

    //hide the UI (for when not in the shop)
    private void Hide()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        PlayerController.canMove = true;
    }

    //show the UI (for when player enters shop)
    public void Show()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        PlayerController.canMove = false;
    }

    //automatically bind event
    void OnEnable()
    {
        if (_btnClose != null)
        {
            _btnClose.clicked += Hide;
        }
    }

    //automatically unbind event
    void OnDisable()
    {
        if (_btnClose != null)
        {
            _btnClose.clicked -= Hide;
        }
    }


}
