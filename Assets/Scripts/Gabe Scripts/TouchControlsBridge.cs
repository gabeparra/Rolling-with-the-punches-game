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
#else
    // No-op on native/editor builds — touch overlay is a WebGL-only thing.
    private static void AIFG_SetTouchControls(int visible) { }
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
        bool gameplay = IsGameplayScene(to.name);
        AIFG_SetTouchControls(gameplay ? 1 : 0);
    }

    private static bool IsGameplayScene(string name)
    {
        // Whitelist gameplay scenes. Menu Screen / SampleScene stay click-driven.
        // HubScene is included so phone players can move around the hub too.
        return name == "Hector Scene"
            || name == "Snow scene"
            || name == "Mountain scene"
            || name == "HubScene";
    }
}
