using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance { get; private set; }

    private TextMeshProUGUI _goldText;
    private TextMeshProUGUI _promptText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Create or find a canvas for the HUD
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("HubCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        Transform root = canvas.transform;

        // Gold display — top-right
        int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        _goldText = CreateLabel(root, "GoldDisplay", "Gold: " + gold, 28,
            new Vector2(0.95f, 0.95f), new Vector2(0.95f, 0.95f), new Vector2(200, 40));
        _goldText.alignment = TextAlignmentOptions.Right;

        // Prompt text — bottom-center, hidden by default
        _promptText = CreateLabel(root, "PromptText", "", 32,
            new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(600, 50));
        _promptText.alignment = TextAlignmentOptions.Center;
        _promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_goldText != null && GameManager.Instance != null)
            _goldText.text = "Gold: " + GameManager.Instance.Gold;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // --- Public prompt API ---

    public static void ShowPrompt(string text)
    {
        if (Instance == null || Instance._promptText == null) return;
        Instance._promptText.text = text;
        Instance._promptText.gameObject.SetActive(true);
    }

    public static void HidePrompt()
    {
        if (Instance == null || Instance._promptText == null) return;
        Instance._promptText.gameObject.SetActive(false);
    }

    // --- Helpers ---

    private TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return tmp;
    }
}
