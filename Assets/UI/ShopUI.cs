using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    //list of upgrades available in shop
    [SerializeField]
    private List<Upgrade> upgrades = new();

    //icon to use for cash (you shouldn't have to mess with this. this is just easier than importing it through code)
    [SerializeField]
    private Background cashIcon;

    private UIDocument uiDocument;
    private Button _btnClose;
    private ListView _list;
    private Label _lblCurrency;

    void Awake()
    {
        //initialize references to UI objects
        uiDocument = GetComponent<UIDocument>();
        _btnClose = uiDocument.rootVisualElement.Q("close") as Button;
        _list = uiDocument.rootVisualElement.Q("listView") as ListView;
        _lblCurrency = uiDocument.rootVisualElement.Q<Label>("cash");
        _list.itemsSource = upgrades;

        Hide(); //hide shop UI until player actually goes to shop

        _list.bindItem = (e, i) =>
        {
            Upgrade u = upgrades[i];
            Button buyBtn = e.Q<Button>("btn"); //get reference to this element's puchase button
            buyBtn.dataSource = u; //explicitly assign the correct data source for this button (used in its onClick)
            buyBtn.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??

            // all other data (upgrade info from SO) is automatically assigned to the element's parts (they are manually bound in ShopItem.uxml)

            //Section for refreshing button text
            int cost = UpgradeManager.GetCost(u);
            if(cost > 0)
            {
                buyBtn.text = cost.ToString(); //initialize with the current price
                buyBtn.iconImage = cashIcon;
                buyBtn.SetEnabled(true);
            }
            else
            {
                buyBtn.text = " - ";
                buyBtn.SetEnabled(false);
                buyBtn.iconImage = null;
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
        if(UpgradeManager.BuyUpgrade(_up))
        {
            //TODO: play success sound
        }
        else
        {
            //TODO: play fail sound
        }

        RefreshData(); // refreshing either way, just in case there was a display error that made the player think they would be able to afford the upgrade
    }


    // hide the UI (for when not in the shop)
    public void Hide()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        //save upon closing, but be sure to only do this if the GameManager has initialized (to avoid accidentally resetting savefile)
        if(GameManager.Ready())
            GameManager.Save();
        TrainPlayerController.canMove = true;
    }

    public void Show()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        RefreshData();
        TrainPlayerController.canMove = false;
    }

    private void RefreshData()
    {
        _list.RefreshItems();
        _lblCurrency.text = $"Cash: {GameManager.getCurrency()}";
        if (HubManager.Instance != null) HubManager.Instance.Refresh();
    }

    //automatically bind event
    void OnEnable()
    {
        if (_btnClose != null)
            _btnClose.clicked += Hide;
        
        RefreshData(); //have to wait until OnEnable to contact GameManager
    }

    //automatically unbind event
    void OnDisable()
    {
        if (_btnClose != null)
            _btnClose.clicked -= Hide;
    }
}
