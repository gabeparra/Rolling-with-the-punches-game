#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Headless Android build entry point for CLI invocation:
///   unity-editor -batchmode -quit -projectPath . -executeMethod AndroidBuilder.BuildAndroid -logFile -
/// Also exposes a Tools menu item for in-editor use.
/// </summary>
public static class AndroidBuilder
{
    const string OUTPUT = "Builds/Android/rolling-punches.apk";
    const string PACKAGE = "dev.gabeparra.rollingpunches";

    [MenuItem("Tools/Build Android APK")]
    public static void BuildAndroid()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[AndroidBuilder] No scenes enabled in Build Settings.");
            EditorApplication.Exit(2);
            return;
        }

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PACKAGE);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        // Debug-keystore signing (Unity default when no custom keystore is set)
        // keeps installs compatible with the rest of the sideloaded fleet.
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.forceInternetPermission = true;

        // Landscape only — the touch layout assumes wide aspect; never rotate
        // to portrait even if the phone does.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OUTPUT,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None,
        };

        Debug.Log($"[AndroidBuilder] Building {scenes.Length} scenes -> {OUTPUT}");

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary s = report.summary;

        Debug.Log($"[AndroidBuilder] result={s.result} size={s.totalSize / (1024 * 1024)}MB " +
                  $"errors={s.totalErrors} warnings={s.totalWarnings} duration={s.totalTime}");

        if (s.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[AndroidBuilder] Build FAILED: {s.result}");
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }
}
#endif
