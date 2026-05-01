using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TutorialManager : MonoBehaviour
{
    private static TutorialManager Instance;

    private UIDocument uiDocument;
    private Label lbl_title;
    private Label lbl_content;
    private Button btn_prev;
    private Button btn_next;
    private Button btn_close;
    private Image image;

    private string[] title = {
        "Welcome to the Wild West!", //exposition
        "Get to Work!", //explain how to start a run
        "Bandits!", //explain goal of bandits
        "Get Shooting!", //explain controls
        "Payday!", //explain payment and getting upgrades
        "Gold Rush!", //final summary on game loop
    };
    private string[] content = {
        "You've just been hired as a train conductor for a freight company based in a sprouting canyonside boomtown. For some reason the freight company is having a hard time keeping the position filled. You've heard reports of bandit raids along the railroad.\nSurely it won't be that bad... Right?",
        "Head to the train station to start your first trip as a conductor.\nYou'll be back...",
        "Your new employers issued you a revolver for self defense. If any stowaways are on board, you won't be allowed to approach the next station.\nLooks like you'll have to do some dirty work",
        "",
        "Get your share of the payment from your trip. The company will still compensate you for your troubles even if your trip is unsuccessful. However, they still must punsh you somehow, so you recieve half the payment you would have otherwise.\nHead to the Supply Post to spend your cash on upgrades to make your job easier.",
        "After your shopping spree, head back to work. Do your best to fulfill the contract, come back here to get more upgrades, and keep that cycle going.\nGo for a high score and strike it rich!",
    };

    private readonly int IMAGE_INDEX = 3;

    private int currentPage = 0;
    private int maxPage = 3;

    private void Awake()
    {
        //check if a TutorialManager instance already exists
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject); //this object is a duplicate; delete it
            return;
        }
        else //this is the object we want to use as our singleton
        {
            Instance = this;
        }

        //ensure no out-of-bounds
        maxPage = Math.Min(title.Length, content.Length) - 1;


        //initialize references to UI objects
        uiDocument = GetComponent<UIDocument>();
        lbl_title = uiDocument.rootVisualElement.Q("title") as Label;
        lbl_content = uiDocument.rootVisualElement.Q("content") as Label;
        btn_prev = uiDocument.rootVisualElement.Q("btn_prev") as Button;
        btn_next = uiDocument.rootVisualElement.Q("btn_next") as Button;
        btn_close = uiDocument.rootVisualElement.Q("btn_close") as Button;
        image = uiDocument.rootVisualElement.Q("image") as Image;

        Refresh();

        Hide(); //hide until needed

        btn_prev.RegisterCallback<ClickEvent>(OnPrevious, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??
        btn_next.RegisterCallback<ClickEvent>(OnNext, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??
        btn_close.RegisterCallback<ClickEvent>(OnFinishTutorial, TrickleDown.NoTrickleDown); //register the onClick //TODO: do we wanta different TrickleDown??

    }

    private void OnPrevious(ClickEvent evt)
    {
        currentPage--;
        Refresh();
    }

    private void OnNext(ClickEvent evt)
    {
        currentPage++;
        Refresh();
    }

    private void OnFinishTutorial(ClickEvent evt)
    {
        GameManager.SetTutorial(true);
        Hide();
    }

    private void Refresh()
    {
        //error check (assume completed to avoid softlock)
        if (currentPage < 0 || maxPage < currentPage) OnFinishTutorial(null);

        //set vis of prev (enabled everywhere except 0) (enabled when currentpage not 0)
        btn_prev.SetEnabled(currentPage != 0);

        //set vis of next (enabled everywhere except maxPage) (enabled with currentpage not maxpage)
        btn_next.SetEnabled(currentPage != maxPage);

        //set vis of close (enabled only on maxPage) (enabled when currentpage == maxpage) also if player has seen tutorials already
        btn_close.SetEnabled(GameManager.CheckTutorial() || currentPage == maxPage);

        //only show image when at imageindex
        image.style.display = (currentPage == IMAGE_INDEX)? DisplayStyle.Flex : DisplayStyle.None;

        //set content
        lbl_title.text = title[currentPage];
        lbl_content.text = content[currentPage];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Ready())
        {
            bool seen = GameManager.CheckTutorial();
            if(!seen)
            {
                Show();
            }
        }
    }

    // hide the UI
    public void Hide()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        TrainPlayerController.canMove = true;
    }

    public void Show()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        TrainPlayerController.canMove = false;
    }
}
