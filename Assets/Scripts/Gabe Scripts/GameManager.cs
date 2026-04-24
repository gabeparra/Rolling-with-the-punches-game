using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private static SaveObject metaSave;

    [SerializeField] private List<Upgrade> allUpgrades = new List<Upgrade>();

    // --- Gold ---
    public int Gold => metaSave != null ? metaSave.goldAmount : 0;

    public void AddGold(int amount)
    {
        if (metaSave == null) return;
        metaSave.goldAmount += amount;
        Save();
    }

    public bool SpendGold(int amount)
    {
        if (metaSave == null || metaSave.goldAmount < amount) return false;
        metaSave.goldAmount -= amount;
        Save();
        return true;
    }

    // --- Upgrades ---
    public bool HasUpgrade(string id)
    {
        return metaSave != null && metaSave.purchasedUpgrades.Contains(id);
    }

    public bool BuyUpgrade(string id, int cost)
    {
        if (HasUpgrade(id)) return false;
        if (!SpendGold(cost)) return false;
        metaSave.purchasedUpgrades.Add(id);
        Save();
        return true;
    }

    // --- Level Progression ---
    // 0 = Western (Hector Scene), 1 = Snow, 2 = Mountain
    public const int MaxLevel = 3;

    public int GetCurrentLevel()
    {
        return metaSave != null ? metaSave.currentLevel : 0;
    }

    public void AdvanceLevel()
    {
        if (metaSave == null) return;
        if (metaSave.currentLevel < MaxLevel)
            metaSave.currentLevel++;
        Save();
    }

    public bool IsGameFinished()
    {
        return metaSave != null && metaSave.currentLevel >= MaxLevel;
    }

    public void ResetProgression()
    {
        if (metaSave == null) return;
        metaSave.currentLevel = 0;
        Save();
    }

    public void ResetAll()
    {
        metaSave = new SaveObject();
        Save();
    }

    // --- Lifecycle ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystem.Init();
        Load();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(metaSave);
        SaveSystem.Save(json);
    }

    public void Load()
    {
        string json = SaveSystem.Load();
        if (json != null)
        {
            metaSave = JsonUtility.FromJson<SaveObject>(json);
            if (metaSave.purchasedUpgrades == null)
                metaSave.purchasedUpgrades = new List<string>();
            Debug.Log("Loaded save: " + json);
        }
        else
        {
            metaSave = new SaveObject();
            Save();
        }
    }

    [System.Serializable]
    private class SaveObject
    {
        public int goldAmount = 0;
        public List<string> purchasedUpgrades = new List<string>();
        public int currentLevel = 0;
    }
}
