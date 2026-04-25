using UnityEngine;

/// <summary>
/// Visually swaps between Gold Crate variant children based on the player's
/// current gold (read from GameManager). Tier breakpoints default to every
/// 100 gold: Empty for &lt;100, One Bar for 100-199, ... Six Bars for 600+.
/// Reverses automatically when gold is spent. Read-only — does not modify gold.
///
/// Setup:
///  - Place this component on an empty wrapper GameObject (e.g. Crate 1).
///  - Add the 7 Gold Crate prefab variants as children of that wrapper.
///  - Assign each child to the variants[] slots in tier order:
///      0 = Gold Crate Empty
///      1 = Gold Crate One Bar
///      2 = Gold Crate Two Bars
///      3 = Gold Crate Three Bars
///      4 = Gold Crate Four Bars
///      5 = Gold Crate Five Bars
///      6 = Gold Crate Six Bars
/// </summary>
public class GoldCrateDisplay : MonoBehaviour
{
    [Header("Variants (tier order: Empty, OneBar, TwoBars, ... SixBars)")]
    [Tooltip("Drag the variant child GameObjects here in tier order. Index 0 = Empty.")]
    [SerializeField] private GameObject[] variants = new GameObject[7];

    [Header("Tuning")]
    [Tooltip("Gold required per tier step. 100 means Empty<100, OneBar 100-199, etc.")]
    [SerializeField] private int goldPerTier = 100;

    // -1 forces the first ApplyTier to write through, even if the computed
    // tier happens to be 0 (which is the array's default int value).
    private int _currentTier = -1;

    void Start()
    {
        // Snap to the correct variant immediately on scene load, even if
        // GameManager isn't ready yet (will fall back to tier 0 = Empty).
        ApplyTier(ComputeTier(GetGold()), force: true);
    }

    void Update()
    {
        int tier = ComputeTier(GetGold());
        if (tier != _currentTier)
            ApplyTier(tier, force: false);
    }

    private int GetGold()
    {
        return GameManager.Instance != null ? GameManager.Instance.Gold : 0;
    }

    private int ComputeTier(int gold)
    {
        if (variants == null || variants.Length == 0) return 0;
        int divisor = Mathf.Max(1, goldPerTier);
        return Mathf.Clamp(gold / divisor, 0, variants.Length - 1);
    }

    private void ApplyTier(int tier, bool force)
    {
        if (!force && tier == _currentTier) return;
        if (variants == null) return;

        for (int i = 0; i < variants.Length; i++)
        {
            GameObject v = variants[i];
            if (v == null) continue;
            bool shouldBeActive = (i == tier);
            if (v.activeSelf != shouldBeActive)
                v.SetActive(shouldBeActive);
        }

        _currentTier = tier;
    }
}
