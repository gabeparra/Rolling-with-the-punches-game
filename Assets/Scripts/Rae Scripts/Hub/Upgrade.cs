using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade : ScriptableObject
{
    public string upgradeName;
    public string description;
    public int cost;
    public UpgradeType type;
    [Tooltip("Effect value (e.g. 0.25 = 25% reload-speed reduction)")]
    public float value;

    public enum UpgradeType
    {
        ReloadSpeed,
        Damage,
        MaxAmmo
    }
}
