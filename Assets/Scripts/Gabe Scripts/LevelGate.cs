using UnityEngine;

/// <summary>
/// Attach to each level sign/portal in the Hub.
/// Only enables this GameObject when the player's current progression matches requiredLevel.
/// 0 = Western, 1 = Snow, 2 = Mountain.
/// </summary>
public class LevelGate : MonoBehaviour
{
    [Tooltip("Which level index this gate requires (0=Western, 1=Snow, 2=Mountain)")]
    public int requiredLevel;

    void Start()
    {
        UpdateGate();
    }

    public void UpdateGate()
    {
        if (GameManager.Instance == null) return;
        int current = GameManager.Instance.GetCurrentLevel();
        gameObject.SetActive(current == requiredLevel);
    }
}
