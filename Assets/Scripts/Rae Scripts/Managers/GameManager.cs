using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*got this from google so might not be best*/
    //public static property to access the single instance of this class
    private static GameManager Instance;
    private static SaveObject metaSave; //hold meta progression data while the game is open
    private static RunObject runSave = null; //hold run data (player health, current level)
    private static bool runCurrencyMode = false; //set to true while in a run (use the function to check this, to null-check the runSave)

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

        //TODO: check if we're in a run (look for file) and try to resume it

    }

    /// <summary>
    /// tells you whether the game's currency mode is the run-specific gold
    /// </summary>
    /// <returns>true if in runCurrencyMode</returns>
    public static bool getCurrencyMode()
    {
        return runSave != null && runCurrencyMode == true;
    }

    /// <summary>
    /// Deletes and recreates the player's META savefile. Ensures the SaveSystem is initialized in the process
    /// </summary>
    public static void ResetSave()
    {
        metaSave = new();
        SaveSystem.Init();
        Save();
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
        int currency = getCurrencyMode()? runSave.goldAmount : metaSave.cashAmount;

        int temp = currency + difference;

        // don't apply the update if currency would be negative (can't afford something)
        if(temp < 0)
            return false;

        // can afford: apply the change to the correct currency
        if(getCurrencyMode())
            runSave.goldAmount = temp;
        else
            metaSave.cashAmount = temp;
        return true;
    }

    public static int getCurrency()
    {
        if(getCurrencyMode())
            return runSave.goldAmount;
        return metaSave.cashAmount;
    }

    /// <summary>
    /// modify the current health of the player
    /// </summary>
    /// <param name="difference">the amount to change the player's health by. Negative will subtract</param>
    /// <returns>false if not in a run, or player is now dead. True if successful</returns>
    public static bool UpdatePlayerHealth(int difference)
    {
        if (runSave == null) return false; //make sure we're in a run

        int temp = runSave.playerHealth + difference;

        //this will kill the player
        if(temp <= 0)
        {
            runSave.playerHealth = 0;
            runSave.alive = false;
            return false;
        }

        //successful, player still alive
        runSave.playerHealth = temp;
        return true;
    }

    /// <summary>
    /// use to get the player's current health
    /// </summary>
    /// <returns>the player's current health. Returns zero if not in a run, or player is dead</returns>
    public static int getPlayerHealth()
    {
        if(runSave == null || !runSave.alive) return 0;

        return runSave.playerHealth;
    }

    //TODO: fill out this class with more relevant data
    private class SaveObject
    {
        public int cashAmount = 0;
        //unlocked upgrades (this is upgrades you're allowed to purchase, scrapped)
        public List<UpgradeManager.UpgradeTracker> purchasedUpgrades = new();
        //train inventory?? (scrapped)
        //items in bank?? (scrapped)
    }

    private class RunObject
    {
        public int seed = (int)DateTime.Now.Ticks; //you can use this in random things, to allow for consistency/reproducibility
        public int goldAmount = 0; //initial value is how much you start run with
        public bool alive = true;
        public int playerHealth = PlayerStats.maxHealth;
        public int currentLevel = 1; //TODO should we start with 0 and increment as part of transitioning to a level?
        public int maxLevel = 3; //TODO: change this if we add infinite mode
    }

    /// <summary>
    /// starts a new run by setting runSave to a new RunObject then returning its seed
    /// </summary>
    /// <returns>The seed of the new run</returns>
    public int StartRun()
    {
        runSave = new();
        runCurrencyMode = true;
        return runSave.seed;
    }

    /// <summary>
    /// ends the current run by setting runSave to null (this is how checks for whether we're in a run are done).
    /// Also converts gold to cash, based on player's conversion rate (upgradable)
    /// </summary>
    public void EndRun()
    {
        //make sure we were in a run before continuing
        if(runSave != null) return;

        int runGold = getCurrency(); //at the moment, currency is still set to runSave
        int amountToAdd = (int)(runGold * PlayerStats.currencyRate); //convert that gold to cash
        if(!runSave.alive) amountToAdd /= 2; //reward the player less for returning dead
        //ensure currency mode is set to meta
        runSave = null;
        runCurrencyMode = false;
        UpdateCurrency(amountToAdd); //add the converted gold to the meta save

    }

    public int GetCurrentLevel()
    {
        if (runSave == null) return 0;
        return runSave.currentLevel;
    }

    public int GetMaxLevel()
    {
        if(runSave == null) return 0;
        return runSave.maxLevel;
    }

    /// <summary>
    /// tell the run to increment the level. Fails if not in a run or would exceed the run's maxLevel
    /// </summary>
    /// <returns>true if successful</returns>
    public bool AdvanceLevel()
    {
        //not in a run, failure
        if(runSave == null) return false;
        //would exceed maxLevel, failure
        if(runSave.currentLevel + 1 > runSave.maxLevel) return false;
        //successful
        runSave.currentLevel++;
        return true;

    }

    /// <summary>
    /// use to get the run's seed
    /// </summary>
    /// <returns>The run's seed, or 0 if not in a run</returns>
    public int GetRunSeed()
    {
        if(runSave == null) return 0;
        return runSave.seed;
    }

    /// <summary>
    /// tells you whether the player is alive. Assumes false if not in a run
    /// </summary>
    /// <returns></returns>
    public bool GetPlayerAlive()
    {
        if(runSave == null) return false;
        return runSave.alive;
    }

    /// <summary>
    /// tells the run whether the player is alive.
    /// </summary>
    /// <param name="isAlive">whether the player is alive</param>
    /// <returns>true if successful. false if not in a run</returns>
    public bool SetPlayerAlive(bool isAlive)
    {
        if(runSave == null) return false;
        runSave.alive = isAlive;
        return true;
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

        if(arg1.name == "HubScene")
        {
            Debug.Log("We've entered the Hub");
            EndRun();
        }
    }
}
