using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private static UpgradeManager instance;

    private static Dictionary<string, int> purchasedUpgrades = new();

    [Serializable]
    public struct UpgradeTracker
    {
        public string name; //the name of the upgrade to track
        public int level; //the current level of the upgrade

        public UpgradeTracker(string name, int level) : this()
        {
            this.name = name;
            this.level = level;
        }
    }

    public static void BuyUpgrade(Upgrade upgrade)
    {
        int cost = GetCost(upgrade);
        if(GameManager.UpdateCurrency(-cost))
        {
            string _name = upgrade.upgradeName;
            purchasedUpgrades[_name] = purchasedUpgrades.GetValueOrDefault(_name, 0) + 1;

            Debug.Log($"Purchased {_name} for {cost}");
            Debug.Log($"{_name} is now lvl {purchasedUpgrades[_name]}");
        }

        Debug.Log($"You have {GameManager.getCurrency()} cash");

    }

    private void Awake()
    {
        //check whether an instance already exists
        if(instance != null && instance != this)
            Destroy(this.gameObject); //this is a duplicate. destroy it
        else //this is the object we want to use
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public static void SetAll(List<UpgradeTracker> importList)
    {
        purchasedUpgrades = new();
        foreach(UpgradeTracker _up in importList)
        {
            purchasedUpgrades.Add(_up.name, _up.level);
        }
    }

    public static List<UpgradeTracker> GetAll()
    {
        List<UpgradeTracker> result = new();

        foreach(string _name in purchasedUpgrades.Keys)
        {
            UpgradeTracker _up = new(_name, purchasedUpgrades[_name]);
            result.Add(_up);
            Debug.Log(_up);
        }

        return result;
    }

    public static int GetLevel(string upgradeName)
    {
        return purchasedUpgrades.GetValueOrDefault(upgradeName, 0);
    }

    /// <summary>
    /// use to get the cost to purchase the next level of the given upgrade
    /// </summary>
    /// <param name="upgrade">the upgrade to get the cost of</param>
    /// <returns>The cost to purchase the next level of the upgrade. Returns -1 if there are no prices defined for the upgrade, or the player has reached or passed the upgrade's max level. If there more levels to purchase than defined prices, the remaining levels use the last defined price.</returns>
    public static int GetCost(Upgrade upgrade)
    {
        int currentLevel = GetLevel(upgrade.upgradeName);
        int maxLevel = upgrade.maxLevel;
        int pricesLen = upgrade.prices.Length;

        int result;

        Debug.Log($"current level of {upgrade.upgradeName} is {currentLevel}. Its max level is {maxLevel}, and {pricesLen} prices are defined", upgrade);

        //no prices defined, use -1
        if (pricesLen < 0) 
        {
            result = -1;
            Debug.Log($"No prices are defined for {upgrade.upgradeName}. returning {result}");
            return result;
        }

        //we've reached/passed the max level, use -1
        if(currentLevel >= maxLevel)
        {
            result = -1;
            Debug.Log($"We've reached/passed the max level for {upgrade.upgradeName}. returning {result}");
            return result;
        }

        //we've passed the last defined price, use the last one
        if(currentLevel >= pricesLen)
        {
            //if final level is 6, the last upgrade price is stored in prices[5]. At that point, current level = 5, and pricesLen = 6
            //that means generally, we've gone too far if current level = pricesLen
            //the final level is stored in pricesLen-1
            result = upgrade.prices[pricesLen - 1];
            Debug.Log($"We've passed the last defined price for {upgrade.upgradeName}. The final price was {result}");
            return result;
        }

        //nothing is wrong? use matching level
        result = upgrade.prices[GetLevel(upgrade.upgradeName)];
        Debug.Log($"All checks passed. The current price for {upgrade.upgradeName} should be {result}");
        return result;
    }

}
