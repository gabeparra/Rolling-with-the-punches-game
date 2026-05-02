#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Headless WebGL build entry point for CLI invocation:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod WebGLBuilder.BuildWebGL -logFile -
/// Also exposes a Tools menu item for in-editor use.
/// </summary>
public static class WebGLBuilder
{
    const string OUTPUT = "Builds/WebGL";

    [MenuItem("Tools/Build WebGL")]
    public static void BuildWebGL()
    {
        // Use whatever scenes are enabled in Build Settings — keeps the source
        // of truth in one place instead of hard-coding paths here.
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[WebGLBuilder] No scenes enabled in Build Settings.");
            EditorApplication.Exit(2);
            return;
        }

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OUTPUT,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };

        Debug.Log($"[WebGLBuilder] Building {scenes.Length} scenes -> {OUTPUT}");

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary s = report.summary;

        Debug.Log($"[WebGLBuilder] result={s.result} size={s.totalSize / (1024 * 1024)}MB " +
                  $"errors={s.totalErrors} warnings={s.totalWarnings} duration={s.totalTime}");

        if (s.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[WebGLBuilder] Build FAILED: {s.result}");
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }
}
#endif
