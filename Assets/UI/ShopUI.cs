using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    //list of upgrades available in shop
    [SerializeField]
    private List<Upgrade> upgrades = new();

    [SerializeField]
    private Background cashIcon;

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
            Upgrade _upgrade = _list.itemsSource[i] as Upgrade;
            Button _b = e.Q<Button>("btn"); //get reference to this element's puchase button
            _b.dataSource = _upgrade; //explicitly assign the correct data source for this button (used in its onClick)
            _b.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??

            // all other data (upgrade info from SO) is automatically assigned to the element's parts (they are manually bound in ShopItem.uxml)

            //Section for refreshing button text
            int cost = UpgradeManager.GetCost(_upgrade);
            if(cost > 0)
            {
                _b.text = cost.ToString(); //initialize with the current price
                _b.iconImage = cashIcon;
            }
            else
            {
                _b.text = " - ";
                _b.SetEnabled(false);
                _b.iconImage = null;
            }
        };
    }

    private void OnClick(ClickEvent evt)
    {
        string _str;
        Button _btn = evt.target as Button;
        Upgrade _up;
        var data = _btn.GetHierarchicalDataSourceContext().dataSource;
        if(data == null)
        {
            Debug.Log("data source null :(");
            return;
        }
        else
        {
            _up = data as Upgrade;
            _str = _up.upgradeName;
        }

        Debug.Log("button pressed: " + _str);
        UpgradeManager.BuyUpgrade(_up);
        _list.RefreshItems();
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
        _list.RefreshItems();
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
