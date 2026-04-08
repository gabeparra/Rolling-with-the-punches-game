using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*got this from google so might not be best*/
    //public static property to access the single instance of this class
    private static GameManager Instance;
    private static SaveObject metaSave; //hold meta progression data while the game is open

    //awake runs before start
    private void Awake()
    {
        //check if a GameManager instance already exists
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject); //this object is a duplicate; delete it
        }
        else //this is the object we want to use as our singleton
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        //initialize save system and load player's data
        SaveSystem.Init();
        Load();

    }

    public static void Save()
    {
        metaSave.purchasedUpgrades = UpgradeManager.GetAll();
        string json = JsonUtility.ToJson(metaSave);
        SaveSystem.Save(json);

        Debug.Log("Saved!");

    }

    public static void Load()
    {
        string json = SaveSystem.Load();
        if(json != null)
        {
            metaSave = JsonUtility.FromJson<SaveObject>(json);
            UpgradeManager.SetAll(metaSave.purchasedUpgrades);
            Debug.Log("Loaded!");
            Debug.Log(json);
        }
        else //no savefile found, create a blank one
        {
            Debug.Log("no savefile found. a new one will be created");
            metaSave = new SaveObject();
            Save();
        }


    }

    public static bool UpdateCurrency(int difference)
    {
        int temp = metaSave.cashAmount + difference;

        // don't apply the update if currency would be negative (can't afford something)
        if(temp < 0)
            return false;

        metaSave.cashAmount = temp;
        return true;
    }

    public static int getCurrency()
    {
        return metaSave.cashAmount;
    }

    //TODO: fill out this class with more relevant data
    private class SaveObject
    {
        public int cashAmount = 0;
        //unlocked upgrades
        public List<UpgradeManager.UpgradeTracker> purchasedUpgrades = new();
        //train inventory??
        //items in bank??
    }

    private class RunData
    {
        public int goldAmount = 0;
        public bool alive = true;
        //current inventory
        //current level
    }


    /*testing detecting new scene loaded*/
    private void OnEnable()
    {
        Debug.Log("gameManager onEnable");
        SceneManager.activeSceneChanged += OnNewScene;
    }

    private void OnDisable()
    {
        Debug.Log("gameManager ondisable");
        SceneManager.activeSceneChanged -= OnNewScene;
    }

    private void OnNewScene(Scene arg0, Scene arg1)
    {
        Debug.Log("scene1: " + arg0.name);
        Debug.Log("scene2: " + arg1.name);
    }
}
