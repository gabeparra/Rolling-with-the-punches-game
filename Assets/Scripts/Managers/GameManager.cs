using UnityEngine;

public class GameManager : MonoBehaviour
{
    /*got this from google so might not be best*/
    //public static property to access the single instance of this class
    public static GameManager Instance{get; private set;}

    //awake runs before start
    private void Awake()
    {
        //check if an instance already exists
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject); //this object is a duplicate; delete it
        }
        else //this is the object we want to use as our singleton
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {

    // }

    public void Save()
    {
        Debug.Log("game should save");
    }

    public void Load()
    {
        Debug.Log("game should load");
    }
}
