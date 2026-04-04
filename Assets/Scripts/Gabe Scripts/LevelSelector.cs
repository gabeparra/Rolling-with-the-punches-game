using UnityEngine;

/// <summary>
/// Persists across scene loads to tell the gameplay scene which environment theme to use.
/// Set LevelSelector.CurrentTheme before loading "Hector Scene".
/// </summary>
public class LevelSelector : MonoBehaviour
{
    public enum Theme { Western, Snowy, Mountain }

    public static Theme CurrentTheme { get; set; } = Theme.Western;
}
