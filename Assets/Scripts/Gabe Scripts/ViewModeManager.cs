using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ViewMode { Classic, TopDown, FirstPerson }

/// <summary>
/// Combat-scene camera modes: Classic (the original 45° FollowPlayer rig),
/// TopDown (true overhead twin-stick) and FirstPerson (head cam, drag/mouse
/// look). Cycled with the VIEW overlay button on phones or V on desktop;
/// choice persists across scenes and sessions via PlayerPrefs.
///
/// Runtime-hooked like the rest of the mobile layer — no scene edits. The
/// manager only takes over in the three combat scenes; hub and menu keep
/// their own cameras untouched.
/// </summary>
public static class ViewModeManager
{
    // Static ctor (not Init) so Mode is correct no matter which of the two
    // RuntimeInitializeOnLoad bootstraps (this or MobileTouchControls) runs
    // first — the overlay reads Mode while building its FIRE button.
    static ViewModeManager() { Mode = (ViewMode)PlayerPrefs.GetInt("viewMode", 0); }

    public static ViewMode Mode { get; private set; }

    /// <summary>Single source of truth for "first person is live right now".</summary>
    public static bool FpsActive => Mode == ViewMode.FirstPerson && InCombatScene;
    /// <summary>FPS look yaw in degrees — player body faces this.</summary>
    public static float Yaw { get; private set; }
    public static float Pitch { get; private set; }
    /// <summary>True while a combat scene is active and the manager owns the camera.</summary>
    public static bool InCombatScene { get; private set; }

    static Camera _cam;
    static FollowPlayer _follow;
    static TrainPlayerController _player;
    static Renderer[] _playerRenderers;
    static ViewModeDriver _driver;
    static GameObject _crosshair;
    static Quaternion _camHomeRot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
        OnSceneChanged(default, SceneManager.GetActiveScene());
    }

    static void OnSceneChanged(Scene from, Scene to)
    {
        InCombatScene = TouchControlsBridge.IsCombatScene(to.name);
        _cam = null; _follow = null; _player = null; _playerRenderers = null;
        if (!InCombatScene)
        {
            // Leaving combat mid-FPS: the crosshair lives on a persistent
            // canvas and would otherwise stay painted over the hub/menu.
            if (_crosshair != null) _crosshair.SetActive(false);
            return;
        }

        _cam = Camera.main;
        if (_cam != null) _camHomeRot = _cam.transform.rotation;
        _follow = Object.FindFirstObjectByType<FollowPlayer>();
        _player = Object.FindFirstObjectByType<TrainPlayerController>();
        if (_player != null)
            _playerRenderers = _player.GetComponentsInChildren<Renderer>();
        if (_driver == null)
        {
            var go = new GameObject("ViewModeDriver");
            Object.DontDestroyOnLoad(go);
            _driver = go.AddComponent<ViewModeDriver>();
            BuildCrosshair(go);
        }
        Apply();
    }

    public static void Cycle()
    {
        Mode = (ViewMode)(((int)Mode + 1) % 3);
        PlayerPrefs.SetInt("viewMode", (int)Mode);
        if (InCombatScene) Apply();
    }

    static void Apply()
    {
        bool fps = Mode == ViewMode.FirstPerson;
        if (_follow != null) _follow.enabled = Mode == ViewMode.Classic;
        // FollowPlayer only drives position — put the original tilt back when
        // returning from a mode that changed the camera's rotation.
        if (Mode == ViewMode.Classic && _cam != null) _cam.transform.rotation = _camHomeRot;
        if (_playerRenderers != null)
            foreach (var r in _playerRenderers) if (r != null) r.enabled = !fps;
        if (_crosshair != null) _crosshair.SetActive(fps && InCombatScene);
        MobileTouchControls.OnViewModeChanged(Mode);
        if (_player != null) { Yaw = _player.transform.eulerAngles.y; Pitch = 0f; }
    }

    static void BuildCrosshair(GameObject parent)
    {
        _crosshair = new GameObject("Crosshair", typeof(RectTransform));
        var canvas = _crosshair.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 490;
        Object.DontDestroyOnLoad(_crosshair);
        var dot = new GameObject("Dot", typeof(RectTransform));
        dot.transform.SetParent(_crosshair.transform, false);
        var rt = (RectTransform)dot.transform;
        rt.sizeDelta = new Vector2(6, 6);
        rt.anchoredPosition = Vector2.zero;
        var img = dot.AddComponent<Image>();
        img.sprite = MobileTouchControls.MakeCircleSprite(32); // round dot, matches overlay art
        img.color = new Color(1f, 1f, 1f, 0.9f);
        img.raycastTarget = false;
        _crosshair.SetActive(false);
    }

    /// <summary>Where bullets should go in FPS mode (camera ray incl. pitch).</summary>
    public static bool GetFpsAim(out Vector3 origin, out Vector3 direction)
    {
        origin = default; direction = default;
        if (Mode != ViewMode.FirstPerson || _cam == null) return false;
        origin = _cam.transform.position + _cam.transform.forward * 0.3f;
        direction = _cam.transform.forward;
        return true;
    }

    class ViewModeDriver : MonoBehaviour
    {
        const float TouchLookSpeedX = 140f, TouchLookSpeedY = 90f; // deg/sec at full deflection
        const float MouseSens = 0.12f;
        bool _cursorCaptured;

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
                Cycle();

            // Desktop FPS wants a captured cursor — released while paused so
            // the pause menu stays clickable. Written only on transitions so
            // GameUIManager's own cursor management (e.g. Confined on resume)
            // isn't clobbered every frame in the other view modes.
            if (!Application.isMobilePlatform)
            {
                bool capture = FpsActive && !GameUIManager.IsPaused;
                if (capture != _cursorCaptured)
                {
                    _cursorCaptured = capture;
                    Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
                    Cursor.visible = !capture;
                }
            }

            if (!FpsActive || _player == null) return;

            // Look: touch aim-zone drag (rate), mouse delta, or controller stick.
            Vector2 look = Vector2.zero;
            if (MobileTouchControls.AimHeld)
                look = MobileTouchControls.AimVector *
                    new Vector2(TouchLookSpeedX, TouchLookSpeedY) * Time.deltaTime;
            else if (Mouse.current != null && !MobileTouchControls.Active)
                look = Mouse.current.delta.ReadValue() * MouseSens;
            else if (Gamepad.current != null)
                look = Gamepad.current.rightStick.ReadValue() *
                    new Vector2(TouchLookSpeedX, TouchLookSpeedY) * Time.deltaTime;

            Yaw = Mathf.Repeat(Yaw + look.x, 360f);
            Pitch = Mathf.Clamp(Pitch - look.y, -70f, 70f);
            _player.transform.rotation = Quaternion.Euler(0f, Yaw, 0f);
        }

        void LateUpdate()
        {
            if (!InCombatScene || _player == null) return;
            // Camera.main can be null at the instant the scene-load hook ran
            // (tag/activation timing) — recover instead of staying blind.
            if (_cam == null) { _cam = Camera.main; if (_cam == null) return; }
            switch (Mode)
            {
                case ViewMode.TopDown:
                    _cam.transform.position = _player.transform.position + Vector3.up * 20f;
                    _cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                case ViewMode.FirstPerson:
                    _cam.transform.position = _player.transform.position + Vector3.up * 1.6f;
                    _cam.transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);
                    break;
                // Classic: FollowPlayer owns the camera.
            }
        }
    }
}
