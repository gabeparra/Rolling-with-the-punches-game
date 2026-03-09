using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField]
    private Upgrade up1;
    [SerializeField]
    private Upgrade up2;
    [SerializeField]
    private Upgrade up3;

    private ListView _list;
    private List<Upgrade> upgrades;


    void Awake()
    {
        upgrades = new List<Upgrade>
        {
            up1, up2, up3
        };

        var uiDocument = GetComponent<UIDocument>();
        _list = uiDocument.rootVisualElement.Q("listView") as ListView;
        _list.itemsSource = upgrades;
        // _list.makeItem = () => new Label();
        // _list.bindItem = (element, index) =>
        // {
        //     (element as Label).text = upgrades[index].upgradeName;
        // };

        //TODO: bind each upgrade to its shop slot
    }

    void OnEnable()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
