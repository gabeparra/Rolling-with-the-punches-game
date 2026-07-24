using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Native (Android/iOS) touch controls, built at runtime — no scene edits.
///
/// Layout: a move stick bottom-left (Input System OnScreenStick feeding the
/// virtual gamepad's left stick), a full-height floating AIM ZONE covering the
/// right side of the screen (touch anywhere, a stick appears under the finger;
/// drag to aim, hold to auto-fire), and JUMP / DASH / RELOAD / PAUSE / VIEW
/// buttons layered above the zone. The zone replaced a fixed aim stick that
/// proved too easy to miss mid-fight (telemetry showed the right stick engaged
/// in only ~10% of samples).
/// </summary>
public static class MobileTouchControls
{
    /// <summary>True while the touch overlay is visible in the current scene.</summary>
    public static bool Active { get; private set; }

    /// <summary>Aim-zone state: normalized drag vector (magnitude 0..1) and
    /// whether a finger is currently down on the zone. Combat scripts read
    /// these directly — no Input System indirection to go wrong.</summary>
    public static Vector2 AimVector => TouchAimZone.Aim;
    public static bool AimHeld => TouchAimZone.Held;

    static GameObject _root;
    static GameObject _aimZone;
    static GameObject _fireButton;
    static Image _reloadFill;
    static Sprite _circle;
    static bool _combatScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (!Application.isMobilePlatform) return;

        // Without this every touch doubles as mouse0 and taps anywhere would
        // fire the gun.
        Input.simulateMouseWithTouches = false;

        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
        Build();
        OnSceneChanged(default, SceneManager.GetActiveScene());
    }

    static void OnSceneChanged(Scene from, Scene to)
    {
        _combatScene = TouchControlsBridge.IsCombatScene(to.name);
        bool show = TouchControlsBridge.IsGameplayScene(to.name);
        Active = show;
        if (_root != null) _root.SetActive(show);
        // The aim surface would swallow taps meant for the hub's own UI
        // (Shop, Level Select) — combat scenes only. Same for the FPS FIRE
        // button, which must also re-sync here so it can't stay stuck visible
        // in the hub after leaving a run in first person.
        if (_aimZone != null) _aimZone.SetActive(_combatScene);
        if (_fireButton != null)
            _fireButton.SetActive(_combatScene && ViewModeManager.Mode == ViewMode.FirstPerson);
        // A reload interrupted by the scene change would leave its radial
        // fill frozen mid-way on this persistent canvas.
        SetReloadProgress(0f);
        if (show) EnsureEventSystem();
    }

    /// <summary>Called by ViewModeManager when the mode changes: FPS gets a
    /// dedicated FIRE button (the aim zone is busy steering the camera).</summary>
    public static void OnViewModeChanged(ViewMode mode)
    {
        if (_fireButton != null) _fireButton.SetActive(_combatScene && mode == ViewMode.FirstPerson);
    }

    /// <summary>Reload progress for the RELOAD button (0 = idle, 0..1 filling).</summary>
    public static void SetReloadProgress(float t)
    {
        if (_reloadFill != null) _reloadFill.fillAmount = Mathf.Clamp01(t);
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

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _root.AddComponent<GraphicRaycaster>();

        // Aim zone FIRST so later siblings (buttons) win raycasts above it.
        BuildAimZone();

        MakeStick("MoveStick", new Vector2(0, 0), new Vector2(300, 280), "<Gamepad>/leftStick");

        // Right-edge button column — clear of the natural aim-drag area.
        // DASH doubles as the hub's Interact (buttonNorth), like KeyE on web.
        MakeButton("JUMP",   new Vector2(1, 0), new Vector2(-140, 170), 150, "<Gamepad>/buttonSouth");
        MakeButton("DASH",   new Vector2(1, 0), new Vector2(-140, 350), 130, "<Gamepad>/buttonNorth");
        var reload = MakeButton("RELOAD", new Vector2(1, 0), new Vector2(-140, 510), 110, "<Gamepad>/buttonWest");
        _reloadFill = MakeReloadFill(reload);
        MakeButton("PAUSE",  new Vector2(1, 1), new Vector2(-90, -90), 100, "<Gamepad>/start");

        var view = MakeButton("VIEW", new Vector2(1, 1), new Vector2(-220, -90), 100, null);
        view.AddComponent<Button>().onClick.AddListener(ViewModeManager.Cycle);

        // FPS-only trigger; hidden in other modes (aim zone fires there).
        // Visibility comes from the persisted mode, not a blanket false —
        // ViewModeManager's own bootstrap may already have run and its
        // OnViewModeChanged call would have hit a null button.
        _fireButton = MakeButton("FIRE", new Vector2(1, 0), new Vector2(-330, 210), 170, "<Gamepad>/rightTrigger");
        _fireButton.SetActive(_combatScene && ViewModeManager.Mode == ViewMode.FirstPerson);
    }

    static void BuildAimZone()
    {
        var zone = new GameObject("AimZone", typeof(RectTransform));
        _aimZone = zone;
        zone.transform.SetParent(_root.transform, false);
        var rt = (RectTransform)zone.transform;
        rt.anchorMin = new Vector2(0.42f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = zone.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0f); // invisible but raycastable
        img.raycastTarget = true;
        var tz = zone.AddComponent<TouchAimZone>();

        // Floating stick visual, hidden until a finger lands.
        var bg = MakeCircleImage("AimStickVisual", Vector2.zero, Vector2.zero, 260, new Color(1f, 1f, 1f, 0.12f));
        bg.transform.SetParent(zone.transform, false);
        bg.GetComponent<Image>().raycastTarget = false;
        var knob = MakeCircleImage("Knob", Vector2.one * 0.5f, Vector2.zero, 110, new Color(1f, 0.75f, 0.2f, 0.5f));
        knob.transform.SetParent(bg.transform, false);
        knob.GetComponent<Image>().raycastTarget = false;
        bg.SetActive(false);
        tz.visual = (RectTransform)bg.transform;
        tz.knob = (RectTransform)knob.transform;
    }

    /// <summary>Floating twin-stick aim surface. Tracks a single pointer;
    /// exposes the drag vector and held state as statics.</summary>
    public class TouchAimZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static Vector2 Aim { get; private set; }
        public static bool Held { get; private set; }

        public RectTransform visual;
        public RectTransform knob;

        // Reference-resolution (1080p canvas) units, not raw screen pixels —
        // divided by the canvas scale factor per event so the same physical
        // thumb travel gives the same deflection on any DPI.
        const float Deadzone = 14f;
        const float FullDeflect = 85f;

        int _pointer = int.MinValue;
        Vector2 _origin;
        Canvas _canvas;

        void Awake() { _canvas = GetComponentInParent<Canvas>(); }

        float ToCanvasUnits => _canvas != null && _canvas.scaleFactor > 0f
            ? 1f / _canvas.scaleFactor : 1f;

        public void OnPointerDown(PointerEventData e)
        {
            if (_pointer != int.MinValue) return; // one finger owns the zone
            _pointer = e.pointerId;
            _origin = e.position;
            Held = true; Aim = Vector2.zero;
            if (visual != null)
            {
                visual.gameObject.SetActive(true);
                PlaceOnCanvas(visual, e.position);
                if (knob != null) knob.anchoredPosition = Vector2.zero;
            }
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _pointer) return;
            Vector2 delta = (e.position - _origin) * ToCanvasUnits;
            float mag = delta.magnitude;
            Aim = mag < Deadzone ? Vector2.zero
                : delta / mag * Mathf.Clamp01((mag - Deadzone) / (FullDeflect - Deadzone));
            if (knob != null)
                knob.anchoredPosition = Aim * FullDeflect;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _pointer) return;
            Release();
        }

        // The OS can eat the pointer-up (notification shade, incoming call,
        // app switch) — without these resets the zone would keep auto-firing
        // in a frozen direction and refuse all new touches.
        void OnDisable() { Release(); }
        void OnApplicationFocus(bool focused) { if (!focused) Release(); }
        void OnApplicationPause(bool paused) { if (paused) Release(); }

        void Release()
        {
            _pointer = int.MinValue;
            Held = false; Aim = Vector2.zero;
            if (visual != null) visual.gameObject.SetActive(false);
        }

        void PlaceOnCanvas(RectTransform target, Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, screenPos, null, out Vector2 local);
            target.anchoredPosition = local;
        }
    }

    static void MakeStick(string name, Vector2 corner, Vector2 pos, string controlPath)
    {
        var bg = MakeCircleImage(name, corner, pos, 320, new Color(1f, 1f, 1f, 0.15f));
        bg.transform.SetParent(_root.transform, false);
        bg.GetComponent<Image>().raycastTarget = false;

        var knob = MakeCircleImage("Knob", Vector2.one * 0.5f, Vector2.zero, 140, new Color(1f, 1f, 1f, 0.45f));
        knob.transform.SetParent(bg.transform, false);
        knob.GetComponent<Image>().raycastPadding = new Vector4(-90, -90, -90, -90);

        var stick = knob.AddComponent<OnScreenStick>();
        stick.controlPath = controlPath;
        stick.movementRange = 90;
    }

    static GameObject MakeButton(string label, Vector2 corner, Vector2 pos, float size, string controlPath)
    {
        var go = MakeCircleImage(label, corner, pos, size, new Color(1f, 0.75f, 0.2f, 0.35f));
        go.transform.SetParent(_root.transform, false);
        if (controlPath != null)
        {
            var btn = go.AddComponent<OnScreenButton>();
            btn.controlPath = controlPath;
        }

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
        return go;
    }

    static Image MakeReloadFill(GameObject reloadButton)
    {
        var go = new GameObject("Fill", typeof(RectTransform));
        go.transform.SetParent(reloadButton.transform, false);
        go.transform.SetAsFirstSibling(); // under the label
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.sprite = _circle;
        img.color = new Color(1f, 1f, 1f, 0.55f);
        img.raycastTarget = false;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillAmount = 0f;
        return img;
    }

    static GameObject MakeCircleImage(string name, Vector2 corner, Vector2 pos, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
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

    /// <summary>Soft-edged white circle generated in code so the overlay
    /// needs no art assets. Public: the FPS crosshair reuses it.</summary>
    public static Sprite MakeCircleSprite(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float r = res / 2f - 1f;
        var c = new Vector2(res / 2f, res / 2f);
        var pixels = new Color32[res * res];
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
            byte a = (byte)(255 * Mathf.Clamp01(r - d));
            pixels[y * res + x] = new Color32(255, 255, 255, a);
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }
}
