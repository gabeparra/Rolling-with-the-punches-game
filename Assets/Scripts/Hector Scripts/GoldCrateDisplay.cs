using UnityEngine;

public class GoldCrateDisplay : MonoBehaviour
{
    [Header("Variants (tier order: Empty, OneBar, TwoBars, ... SixBars)")]
    [Tooltip("Drag the variant child GameObjects here in tier order. Index 0 = Empty.")]
    [SerializeField] private GameObject[] variants = new GameObject[7];

    [Header("Tuning")]
    [Tooltip("Gold required per tier step. 100 means Empty<100, OneBar 100-199, etc.")]
    [SerializeField] private int goldPerTier = 100;

    private int _currentTier = -1;

    void Start()
    {
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
        return GameManager.getCurrency();
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
