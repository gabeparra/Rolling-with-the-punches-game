using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    //list of upgrades available in shop TODO: make this one single list rather than several items
    [SerializeField]
    private Upgrade up1;
    [SerializeField]
    private Upgrade up2;
    [SerializeField]
    private Upgrade up3;

    private UIDocument uiDocument;
    private Button _btnClose;
    private ListView _list;
    private List<Upgrade> upgrades;


    void Awake()
    {
        //list of upgrades available (temporary, for testing, until list is a serialized field)
        upgrades = new List<Upgrade>
        {
            up1, up2, up3
        };

        //initialize references to UI objects
        uiDocument = GetComponent<UIDocument>();
        _btnClose = uiDocument.rootVisualElement.Q("close") as Button;
        _list = uiDocument.rootVisualElement.Q("listView") as ListView;
        _list.itemsSource = upgrades;

        Hide(); //hide shop UI until player actually goes to shop


        // _list.makeItem = () => new Label();
        // _list.bindItem = (element, index) =>
        // {
        //     (element as Label).text = upgrades[index].upgradeName;
        // };

        //TODO: bind each upgrade to its shop slot
    }

    //hide the UI (for when not in the shop)
    private void Hide()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    //show the UI (for when player enters shop)
    public void Show()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
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
