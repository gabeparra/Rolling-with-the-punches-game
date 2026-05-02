using System.Collections.Generic;
using UnityEngine;

public class GoldChest : MonoBehaviour
{
    public static readonly List<GoldChest> All = new();

    [Tooltip("Gold remaining in the chest. -1 means unlimited.")]
    public int gold = -1;

    int looters;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public bool IsEmpty => gold == 0;
    public int LooterCount => looters;

    public void BeginLoot() { looters++; }
    public void EndLoot() { looters = Mathf.Max(0, looters - 1); }

    public int Withdraw(int amount)
    {
        if (gold < 0) return amount;
        int give = Mathf.Min(gold, amount);
        gold -= give;
        return give;
    }

    public static GoldChest GetNearest(Vector3 from)
    {
        GoldChest best = null;
        float bestSqr = float.PositiveInfinity;
        for (int i = 0; i < All.Count; i++)
        {
            var c = All[i];
            if (c == null || c.IsEmpty) continue;
            float d = (c.transform.position - from).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = c; }
        }
        return best;
    }
}
