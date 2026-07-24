using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Toggles the page-level body.gameplay class on every scene change so the
/// HTML/CSS overlay shows touch joysticks + buttons in gameplay scenes only.
/// On Menu Screen / HubScene the page is click-driven and the overlay just
/// gets in the way.
///
/// Hooked once via RuntimeInitializeOnLoad — no Inspector setup, no scene
/// edits required.
/// </summary>
public static class TouchControlsBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void AIFG_SetTouchControls(int visible);
    [DllImport("__Internal")]
    private static extern void AIFG_SetCombat(int active);
#else
    // No-op on native/editor builds — touch overlay is a WebGL-only thing.
    private static void AIFG_SetTouchControls(int visible) { }
    private static void AIFG_SetCombat(int active) { }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        // Re-attach is safe: if domain reload re-runs Init, the previous
        // delegate is gone too. Single subscription wins.
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
        // Fire once for the initial scene since activeSceneChanged only
        // fires on transitions, not for the first load.
        OnSceneChanged(default, SceneManager.GetActiveScene());
    }

    private static void OnSceneChanged(Scene from, Scene to)
    {
        // Two independent signals, intentionally not the same set of scenes:
        //   gameplay — show the touch joystick + button overlay. Includes
        //     HubScene so phone players can walk around.
        //   combat   — the page swallows raw canvas touches so taps don't
        //     drift the player aim/fire. EXCLUDES HubScene because the hub
        //     is full of Unity UI buttons (Shop, Level Select) that need
        //     to receive taps; combat scenes don't have tap-target UI.
        AIFG_SetTouchControls(IsGameplayScene(to.name) ? 1 : 0);
        AIFG_SetCombat       (IsCombatScene  (to.name) ? 1 : 0);
    }

    public static bool IsGameplayScene(string name)
    {
        // Whitelist scenes that show the touch overlay. Menu Screen /
        // SampleScene stay click-driven.
        return name == "Hector Scene"
            || name == "Snow scene"
            || name == "Mountain scene"
            || name == "HubScene";
    }

    public static bool IsCombatScene(string name)
    {
        // Combat scenes only — where bare-canvas taps would drift the aim.
        // HubScene is intentionally NOT here so taps reach Unity UI buttons.
        return name == "Hector Scene"
            || name == "Snow scene"
            || name == "Mountain scene";
    }
}
