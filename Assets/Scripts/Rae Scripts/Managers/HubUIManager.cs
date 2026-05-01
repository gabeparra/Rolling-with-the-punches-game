using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HubUIManager : MonoBehaviour
{

    private static HubUIManager Instance;

    private UIDocument uiDocument;
    private Button btn_continue;
    private Button btn_menu;
    private Button btn_respawn;
    private Button btn_resetTut;
    private Button btn_quit;

    [SerializeField]
    private bool hideMenuButton = true;

    private bool menuOpen = false;

    private void Awake()
    {
        //check if a HubUIManager instance already exists
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject); //this object is a duplicate; delete it
            return;
        }
        else //this is the object we want to use as our singleton
        {
            Instance = this;
        }


        //initialize references to UI objects
        uiDocument = GetComponent<UIDocument>();
        btn_continue = uiDocument.rootVisualElement.Q("btn_continue") as Button;
        btn_menu = uiDocument.rootVisualElement.Q("btn_menu") as Button;
        btn_respawn = uiDocument.rootVisualElement.Q("btn_respawn") as Button;
        btn_resetTut = uiDocument.rootVisualElement.Q("btn_resetTut") as Button;
        btn_quit = uiDocument.rootVisualElement.Q("btn_quit") as Button;

        Hide(); //hide until needed

        //disable mainMenu button til it's fixed
        if(hideMenuButton) btn_menu.style.display = DisplayStyle.None;

        btn_continue.RegisterCallback<ClickEvent>(OnContinue, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??
        btn_menu.RegisterCallback<ClickEvent>(OnMenu, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??
        btn_respawn.RegisterCallback<ClickEvent>(OnRespawn, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??
        btn_resetTut.RegisterCallback<ClickEvent>(OnResetTut, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??
        btn_quit.RegisterCallback<ClickEvent>(OnQuit, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??

    }

    private void OnContinue(ClickEvent evt)
    {
        Hide();
    }

    private void OnMenu(ClickEvent evt)
    {
        SceneManager.LoadScene("Menu Screen");
    }

    private void OnRespawn(ClickEvent evt)
    {
        SceneManager.LoadScene("HubScene");
    }

    private void OnResetTut(ClickEvent evt)
    {
        GameManager.SetTutorial(false);
        SceneManager.LoadScene("HubScene");
    }

    private void OnQuit(ClickEvent evt)
    {
        Application.Quit();
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        if(menuOpen) Hide();
        else if(ShopUI.isOpen) //focus on closing the shop first
        {
            ShopUI shopUI = FindAnyObjectByType<ShopUI>();
            if(shopUI != null) shopUI.Hide();
            return;
        }
        else Show(); //there is not another menu to focus on first
    }

    private void OnEnable()
    {
        if(Instance == this)
        {
            InputSystem.actions["Menu"].performed += OnToggle;
        }
    }    

    private void OnDisable()
    {
        if(Instance == this) //should only continue if we're disabling the actual instance of GameManager
        {
            InputSystem.actions["Menu"].performed -= OnToggle;
        }
    }

    // hide the UI
    public void Hide()
    {
        menuOpen = false;
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        TrainPlayerController.canMove = true;
    }

    public void Show()
    {
        GameManager.Save();
        menuOpen = true;
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        TrainPlayerController.canMove = false;
    }
}
