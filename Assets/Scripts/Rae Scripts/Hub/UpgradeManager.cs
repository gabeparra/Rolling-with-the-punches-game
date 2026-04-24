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

}
