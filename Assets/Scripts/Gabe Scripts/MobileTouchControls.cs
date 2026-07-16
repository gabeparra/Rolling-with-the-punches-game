using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Native (Android/iOS) counterpart of the WebGL touch overlay: builds an
/// on-screen move stick, aim stick and JUMP/DASH/SHOOT/RELOAD/PAUSE buttons
/// at runtime — no scene edits, hooked via RuntimeInitializeOnLoad like
/// TouchControlsBridge.
///
/// The controls emulate a virtual gamepad through the Input System's
/// OnScreen components, so everything that already supports a controller
/// (Move/Look/Interact actions, Gamepad.current fallbacks in the combat
/// scripts) responds to touch with no per-script wiring.
/// </summary>
public static class MobileTouchControls
{
    /// <summary>True while the touch overlay is visible in the current scene.
    /// Combat scripts use this to skip mouse-based aim on phones.</summary>
    public static bool Active { get; private set; }

    static GameObject _root;
    static Sprite _circle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (!Application.isMobilePlatform) return;

        // Without this every touch doubles as mouse0 and taps anywhere would
        // fire the gun (the WebGL overlay solved the same problem by
        // swallowing canvas taps).
        Input.simulateMouseWithTouches = false;

        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
        Build();
        OnSceneChanged(default, SceneManager.GetActiveScene());
    }

    static void OnSceneChanged(Scene from, Scene to)
    {
        // Same whitelist as TouchControlsBridge: combat scenes + HubScene so
        // phone players can walk around; Menu Screen stays tap-driven.
        bool show = to.name == "Hector Scene"
                 || to.name == "Snow scene"
                 || to.name == "Mountain scene"
                 || to.name == "HubScene";
        Active = show;
        if (_root != null) _root.SetActive(show);
        if (show) EnsureEventSystem();
    }

    static void EnsureEventSystem()
    {
        if (EventSystem.current != null || Object.FindFirstObjectByType<EventSystem>() != null)
            return;
        var es = new GameObject("TouchEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(es, SceneManager.GetActiveScene());
    }

    static void Build()
    {
        _circle = MakeCircleSprite(128);

        _root = new GameObject("MobileTouchControls");
        Object.DontDestroyOnLoad(_root);

        _root.AddComponent<TouchDebugReporter>(); // TEMP: on-device input diagnostics

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // above gameplay HUD, below nothing that matters
        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _root.AddComponent<GraphicRaycaster>();

        // Sticks: move (left thumb) and aim (right thumb). Aim feeds
        // <Gamepad>/rightStick which the combat aim code already reads.
        MakeStick("MoveStick", new Vector2(0, 0), new Vector2(300, 280), "<Gamepad>/leftStick");
        MakeStick("AimStick", new Vector2(1, 0), new Vector2(-300, 280), "<Gamepad>/rightStick");

        // Buttons — mirrors the WebGL overlay's grid. No SHOOT button: holding
        // the aim stick auto-fires (PlayerShooting reads the right stick).
        // DASH shares a control with the hub's Interact action (buttonNorth)
        // exactly like KeyE did on the web: dash in combat, open shop in hub.
        MakeButton("JUMP",   new Vector2(1, 0), new Vector2(-600, 500), 140, "<Gamepad>/buttonSouth");
        MakeButton("DASH",   new Vector2(1, 0), new Vector2(-380, 560), 140, "<Gamepad>/buttonNorth");
        MakeButton("RELOAD", new Vector2(1, 0), new Vector2(-160, 580), 120, "<Gamepad>/buttonWest");
        MakeButton("PAUSE",  new Vector2(1, 1), new Vector2(-90, -90), 100, "<Gamepad>/start");
    }

    static void MakeStick(string name, Vector2 corner, Vector2 pos, string controlPath)
    {
        var bg = MakeCircleImage(name, corner, pos, 320, new Color(1f, 1f, 1f, 0.15f));
        bg.GetComponent<Image>().raycastTarget = false;

        var knob = MakeCircleImage("Knob", Vector2.one * 0.5f, Vector2.zero, 140, new Color(1f, 1f, 1f, 0.45f));
        knob.transform.SetParent(bg.transform, false);
        // Extend the knob's touch area to cover the whole stick background so
        // grabs don't have to start dead-centre.
        knob.GetComponent<Image>().raycastPadding = new Vector4(-90, -90, -90, -90);

        var stick = knob.AddComponent<OnScreenStick>();
        stick.controlPath = controlPath;
        stick.movementRange = 90;
    }

    static void MakeButton(string label, Vector2 corner, Vector2 pos, float size, string controlPath)
    {
        var go = MakeCircleImage(label, corner, pos, size, new Color(1f, 0.75f, 0.2f, 0.35f));
        var btn = go.AddComponent<OnScreenButton>();
        btn.controlPath = controlPath;

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var rt = (RectTransform)textGo.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var text = textGo.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = Mathf.RoundToInt(size * 0.22f);
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 1f, 1f, 0.9f);
        text.raycastTarget = false;
    }

    static GameObject MakeCircleImage(string name, Vector2 corner, Vector2 pos, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(_root.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = corner; rt.anchorMax = corner;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);
        var img = go.AddComponent<Image>();
        img.sprite = _circle;
        img.color = color;
        return go;
    }

    /// <summary>TEMP diagnostics: posts gamepad/stick state to the phone-bridge
    /// every 2s while the overlay is up, so input plumbing can be debugged from
    /// the dev box without screen access. Remove once touch input is stable.</summary>
    class TouchDebugReporter : MonoBehaviour
    {
        float _next;

        void Update()
        {
            if (!Active || Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 2f;

            var sb = new System.Text.StringBuilder();
            sb.Append("pads=").Append(Gamepad.all.Count)
              .Append(" cur=").Append(Gamepad.current != null ? Gamepad.current.deviceId : -1);
            foreach (var g in Gamepad.all)
                sb.Append(" [").Append(g.deviceId)
                  .Append(" L=").Append(g.leftStick.ReadValue().ToString("F2"))
                  .Append(" R=").Append(g.rightStick.ReadValue().ToString("F2"))
                  .Append(" rt=").Append(g.rightTrigger.ReadValue().ToString("F2")).Append(']');
            var player = FindFirstObjectByType<TrainPlayerController>();
            if (player != null)
                sb.Append(" fwd=").Append(player.transform.forward.ToString("F2"));
            StartCoroutine(Post(sb.ToString()));
        }

        System.Collections.IEnumerator Post(string text)
        {
            var payload = JsonUtility.ToJson(new Ev { text = text });
            using var req = new UnityEngine.Networking.UnityWebRequest(
                "https://fedora.tail747dab.ts.net:9447/event", "POST");
            req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(
                System.Text.Encoding.UTF8.GetBytes(payload));
            req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
        }

        [System.Serializable]
        class Ev { public string device = "rolling-punches"; public string type = "touchdebug"; public string text = ""; }
    }

    /// <summary>Soft-edged white circle generated in code so the overlay
    /// needs no art assets.</summary>
    static Sprite MakeCircleSprite(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float r = res / 2f - 1f;
        var c = new Vector2(res / 2f, res / 2f);
        var pixels = new Color32[res * res];
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
            byte a = (byte)(255 * Mathf.Clamp01(r - d)); // 1px AA edge
            pixels[y * res + x] = new Color32(255, 255, 255, a);
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }
}
