using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade : ScriptableObject
{
    [Tooltip("Unique key used for save tracking (e.g. reload_speed_1)")]
    public string id;
    public string upgradeName;
    public string description;
    public int cost;
    public UpgradeType type;
    [Tooltip("The effect value (e.g. 0.25 means 25% reduction for reload speed)")]
    public float value;

    public enum UpgradeType
    {
        ReloadSpeed,
        Damage,
        MaxAmmo
    }
}
