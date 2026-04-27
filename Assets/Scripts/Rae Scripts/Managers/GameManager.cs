using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*got this from google so might not be best*/
    //public static property to access the single instance of this class
    public static GameManager Instance;
    private static SaveObject metaSave = new(); //hold meta progression data while the game is open
    private static RunObject runSave = null; //hold run data (player health, current level)
    private static bool runCurrencyMode = false; //set to true while in a run (use the function to check this, to null-check the runSave)
    private static bool isReady = false; //sets itself to true once GameManager ready to use

    //awake runs before start
    private void Awake()
    {
        //check if a GameManager instance already exists
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject); //this object is a duplicate; delete it
            return;
        }
        else //this is the object we want to use as our singleton
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        Debug.Log("THIS SHOULD NEVER BE SEEN MORE THAN ONCE");

        //initialize save system and load player's data
        SaveSystem.Init();
        Load();
        isReady = true;

        //TODO: check if we're in a run (look for file) and try to resume it

        // If GameManager is created directly inside a level scene (e.g.
        // pressing Play in editor on a level scene without going through Hub),
        // start a run so currency operations hit run gold, not meta cash.
        if (Instance == this && IsLevelScene(SceneManager.GetActiveScene().name))
            StartRun();
    }

    public static bool Ready()
    {
        return isReady;
    }

    /// <summary>
    /// Returns true if the given scene name is a gameplay-level scene (not the Hub or Menu).
    /// </summary>
    private static bool IsLevelScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        if (sceneName == "HubScene") return false;
        if (sceneName == "Menu Screen" || sceneName == "MenuScreen") return false;
        return true;
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
        //Debug.Log($"Currency mode set to gold? {getCurrencyMode()}");
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
    public static int StartRun()
    {
        Debug.Log("starting a new run");
        runSave = new();
        runCurrencyMode = true;
        return runSave.seed;
    }

    /// <summary>
    /// ends the current run by setting runSave to null (this is how checks for whether we're in a run are done).
    /// Also converts gold to cash, based on player's conversion rate (upgradable)
    /// </summary>
    public static void EndRun()
    {
        Debug.Log("Ending a run");
        //make sure we were in a run before continuing
        if(runSave == null)
        {
            Debug.Log("But we weren't in one");
            return;
        }

        int runGold = getCurrency(); //at the moment, currency is still set to runSave
        int amountToAdd = (int)(runGold * PlayerStats.currencyRate); //convert that gold to cash
        if(!runSave.alive) amountToAdd /= 2; //reward the player less for returning dead
        //ensure currency mode is set to meta
        runSave = null;
        runCurrencyMode = false;
        Debug.Log($"The following should read false: {getCurrencyMode()}");
        Debug.Log($"{getCurrency()} + {amountToAdd} should equal what you see in shop");
        UpdateCurrency(amountToAdd); //add the converted gold to the meta save

    }

    public static int GetCurrentLevel()
    {
        if (runSave == null) return 0;
        return runSave.currentLevel;
    }

    public static int GetMaxLevel()
    {
        if(runSave == null) return 0;
        return runSave.maxLevel;
    }

    /// <summary>
    /// tell the run to increment the level. Fails if not in a run or would exceed the run's maxLevel
    /// </summary>
    /// <returns>true if successful</returns>
    public static bool AdvanceLevel()
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
    public static int GetRunSeed()
    {
        if(runSave == null) return 0;
        return runSave.seed;
    }

    /// <summary>
    /// tells you whether the player is alive. Assumes false if not in a run
    /// </summary>
    /// <returns></returns>
    public static bool GetPlayerAlive()
    {
        if(runSave == null) return false;
        return runSave.alive;
    }

    /// <summary>
    /// tells the run whether the player is alive.
    /// </summary>
    /// <param name="isAlive">whether the player is alive</param>
    /// <returns>true if successful. false if not in a run</returns>
    public static bool SetPlayerAlive(bool isAlive)
    {
        if(runSave == null) return false;
        runSave.alive = isAlive;
        return true;
    }


    /*testing detecting new scene loaded*/
    private void OnEnable()
    {
        Debug.Log("gameManager onEnable");
        SceneManager.sceneLoaded += OnNewScene;
    }

    private void OnDisable()
    {
        if(Instance == this) //should only continue if we're disabling the actual instance of GameManager
        {
            Debug.Log("gameManager ondisable");
            SceneManager.sceneLoaded -= OnNewScene;
        }
    }

    private static void OnNewScene(Scene arg1, LoadSceneMode loadMode)
    {
        //Debug.Log("scene1: " + arg0.name);
        Debug.Log("scene2: " + arg1.name);

        if(arg1.name == "HubScene")
        {
            Debug.Log("We've entered the Hub");
            EndRun();
            return;
        }

        // Entering a gameplay scene — start a run if we don't have one yet so
        // bandit-loot drains run gold (not meta cash) and the HUD shows run gold.
        if (IsLevelScene(arg1.name) && runSave == null)
        {
            Debug.Log("We've entered a level — starting run");
            StartRun();
        }
    }
}
