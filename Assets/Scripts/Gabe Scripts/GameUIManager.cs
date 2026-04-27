using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    private GameObject _pausePanel;
    private GameObject _gameOverPanel;
    private GameObject _winPanel;
    private GameObject _gameFinishedPanel;

    // Game Over / Win stat labels
    private TextMeshProUGUI _goGoldText;
    private TextMeshProUGUI _goKillsText;
    private TextMeshProUGUI _winGoldText;
    private TextMeshProUGUI _winKillsText;

    private bool _anyPanelOpen;

    // First button per panel for controller/keyboard navigation
    private GameObject _pauseFirstBtn;
    private GameObject _gameOverFirstBtn;
    private GameObject _gameFinishedFirstBtn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        IsPaused = false;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Ensure GameManager exists (normally created in HubScene via DontDestroyOnLoad).
        // Without it, level progression breaks and the win flow loops forever.
        if (GameManager.Instance == null)
        {
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // Ensure timeScale is 1 when entering a gameplay scene
        Time.timeScale = 1f;
        IsPaused = false;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("GameUIManager: No Canvas found in scene.");
            return;
        }

        Transform root = canvas.transform;
        _pausePanel = BuildPausePanel(root);
        _gameOverPanel = BuildGameOverPanel(root);
        _winPanel = BuildWinPanel(root);
        _gameFinishedPanel = BuildGameFinishedPanel(root);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_anyPanelOpen && IsPaused)
            {
                // Only allow ESC-resume from the pause panel, not game over/win
                if (_pausePanel.activeSelf)
                    TogglePause();
            }
            else if (!_anyPanelOpen)
            {
                TogglePause();
            }
        }

        // Accept A button / Enter / Space to confirm on result panels
        if (_anyPanelOpen && !IsPaused && ConfirmPressed())
        {
            if (_gameOverPanel.activeSelf)
                RetryLevel();
            else if (_gameFinishedPanel.activeSelf)
            {
                GameManager.ResetSave();
                GoToMainMenu();
            }
        }
    }

    private bool ConfirmPressed()
    {
        // Keyboard
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            return true;
        // Xbox A button (joystick button 0)
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
            return true;
        return false;
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            _pausePanel.SetActive(false);
            _anyPanelOpen = false;
            IsPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _pausePanel.SetActive(true);
            _anyPanelOpen = true;
            IsPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SelectButton(_pauseFirstBtn);
        }
    }

    public void ShowGameOver(int gold, int kills)
    {
        if (_anyPanelOpen) return;
        _anyPanelOpen = true;
        Time.timeScale = 0f;
        if (_goGoldText != null) _goGoldText.text = "Gold: " + gold;
        if (_goKillsText != null) _goKillsText.text = "Kills: " + kills;
        _gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SelectButton(_gameOverFirstBtn);
    }

    public void ShowWin(int gold, int kills)
    {
        if (_anyPanelOpen) return;
        _anyPanelOpen = true;
        Time.timeScale = 0f;

        // Advance level — returns false when at max (we just won the final level).
        bool advanced = GameManager.Instance != null && GameManager.AdvanceLevel();

        if (_winGoldText != null) _winGoldText.text = "Gold: " + gold;
        if (_winKillsText != null) _winKillsText.text = "Kills: " + kills;

        if (!advanced)
        {
            // No further levels — game complete
            _gameFinishedPanel.SetActive(true);
            SelectButton(_gameFinishedFirstBtn);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Show victory briefly then auto-load the next level
            _winPanel.SetActive(true);
            StartCoroutine(AutoLoadNextLevel());
        }
    }

    private IEnumerator AutoLoadNextLevel()
    {
        // Wait 2.5 seconds (real time, since timeScale is 0)
        yield return new WaitForSecondsRealtime(2.5f);

        int level = GameManager.Instance != null ? GameManager.GetCurrentLevel() : 1;

        // Map currentLevel to theme. Rae's GameManager: level 1 = Western (start),
        // 2 = Snow (after first AdvanceLevel), 3 = Mountain (after second).
        LevelSelector.Theme nextTheme;

        switch (level)
        {
            case 2:
                nextTheme = LevelSelector.Theme.Snowy;
                break;
            case 3:
                nextTheme = LevelSelector.Theme.Mountain;
                break;
            default:
                GoToHub();
                yield break;
        }

        LevelSelector.CurrentTheme = nextTheme;
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene("Hector Scene");
    }

    private void SelectButton(GameObject btn)
    {
        if (EventSystem.current != null && btn != null)
            EventSystem.current.SetSelectedGameObject(btn);
    }

    // ── Scene loading helpers ──────────────────────────────────

    private void RetryLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToHub()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene("HubScene");
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene("Menu Screen");
    }

    // ── UI Builder helpers ─────────────────────────────────────

    private GameObject BuildPausePanel(Transform parent)
    {
        GameObject panel = CreateOverlayPanel(parent, "PausePanel", new Color(0, 0, 0, 0.7f));

        CreateTitle(panel.transform, "PAUSED", 60);

        float btnY = -30f;
        _pauseFirstBtn = CreateButton(panel.transform, "ResumeBtn", "Resume", btnY, () => TogglePause()).gameObject;
        CreateButton(panel.transform, "HubBtn_P", "Return to Hub", btnY - 70f, () => GoToHub());
        CreateButton(panel.transform, "MenuBtn_P", "Main Menu", btnY - 140f, () => GoToMainMenu());

        panel.SetActive(false);
        return panel;
    }

    private GameObject BuildGameOverPanel(Transform parent)
    {
        GameObject panel = CreateOverlayPanel(parent, "GameOverPanel", new Color(0.3f, 0, 0, 0.8f));

        CreateTitle(panel.transform, "GAME OVER", 60);

        _goGoldText = CreateStatLabel(panel.transform, "GOGold", "Gold: 0", 10f);
        _goKillsText = CreateStatLabel(panel.transform, "GOKills", "Kills: 0", -30f);

        _gameOverFirstBtn = CreateButton(panel.transform, "RetryBtn", "Retry", -100f, () => RetryLevel()).gameObject;
        CreateButton(panel.transform, "HubBtn_GO", "Return to Hub", -170f, () => GoToHub());

        panel.SetActive(false);
        return panel;
    }

    private GameObject BuildWinPanel(Transform parent)
    {
        GameObject panel = CreateOverlayPanel(parent, "WinPanel", new Color(0, 0.2f, 0, 0.8f));

        CreateTitle(panel.transform, "VICTORY", 60);

        _winGoldText = CreateStatLabel(panel.transform, "WinGold", "Gold: 0", 10f);
        _winKillsText = CreateStatLabel(panel.transform, "WinKills", "Kills: 0", -30f);
        CreateStatLabel(panel.transform, "NextLevel", "Loading next level...", -80f);

        panel.SetActive(false);
        return panel;
    }

    private GameObject BuildGameFinishedPanel(Transform parent)
    {
        GameObject panel = CreateOverlayPanel(parent, "GameFinishedPanel", new Color(0.1f, 0.05f, 0.0f, 0.9f));

        CreateTitle(panel.transform, "GAME FINISHED", 60);
        CreateStatLabel(panel.transform, "FinishedMsg", "You conquered the Wild West!", 10f);

        _gameFinishedFirstBtn = CreateButton(panel.transform, "MenuBtn_F", "Main Menu", -80f, () => {
            GameManager.ResetSave();
            GoToMainMenu();
        }).gameObject;

        panel.SetActive(false);
        return panel;
    }

    private GameObject CreateOverlayPanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.GetComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = true;

        return panel;
    }

    private TextMeshProUGUI CreateTitle(Transform parent, string text, float yOffset)
    {
        GameObject go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600, 80);
        rt.anchoredPosition = new Vector2(0, yOffset + 80f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 54;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return tmp;
    }

    private TextMeshProUGUI CreateStatLabel(Transform parent, string name, string text, float yOffset)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 40);
        rt.anchoredPosition = new Vector2(0, yOffset);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return tmp;
    }

    private Button CreateButton(Transform parent, string name, string label, float yOffset, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);

        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(260, 50);
        rt.anchoredPosition = new Vector2(0, yOffset);

        Image img = btnGo.GetComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button btn = btnGo.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        cb.highlightedColor = new Color(0.5f, 0.45f, 0.2f, 1f);
        cb.selectedColor = new Color(0.5f, 0.45f, 0.2f, 1f);
        cb.pressedColor = new Color(0.6f, 0.5f, 0.1f, 1f);
        btn.colors = cb;

        // Explicit vertical navigation
        Navigation nav = btn.navigation;
        nav.mode = Navigation.Mode.Automatic;
        btn.navigation = nav;
        btn.onClick.AddListener(onClick);

        // Button label
        GameObject txtGo = new GameObject("Label", typeof(RectTransform));
        txtGo.transform.SetParent(btnGo.transform, false);

        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btn;
    }
}
