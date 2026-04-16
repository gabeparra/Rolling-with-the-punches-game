using UnityEngine;
using UnityEngine.SceneManagement;

public class DevLevelSelect : MonoBehaviour
{
    static DevLevelSelect instance;
    bool showMenu = false;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            showMenu = !showMenu;
    }

    void OnGUI()
    {
        if (!showMenu) return;

        float w = 200f, h = 40f, pad = 5f;
        float startX = 10f, startY = 10f;

        GUI.Box(new Rect(startX - 5, startY - 5, w + 10, (h + pad) * 6 + 10), "");

        if (GUI.Button(new Rect(startX, startY, w, h), "Western (Hector Scene)"))
        {
            LevelSelector.CurrentTheme = LevelSelector.Theme.Western;
            SceneManager.LoadScene("Hector Scene");
            showMenu = false;
        }
        startY += h + pad;

        if (GUI.Button(new Rect(startX, startY, w, h), "Mountain"))
        {
            LevelSelector.CurrentTheme = LevelSelector.Theme.Mountain;
            SceneManager.LoadScene("Hector Scene");
            showMenu = false;
        }
        startY += h + pad;

        if (GUI.Button(new Rect(startX, startY, w, h), "Snow"))
        {
            LevelSelector.CurrentTheme = LevelSelector.Theme.Snowy;
            SceneManager.LoadScene("Hector Scene");
            showMenu = false;
        }
        startY += h + pad;

        if (GUI.Button(new Rect(startX, startY, w, h), "Hub"))
        {
            SceneManager.LoadScene("HubScene");
            showMenu = false;
        }
        startY += h + pad;

        if (GUI.Button(new Rect(startX, startY, w, h), "Main Menu"))
        {
            SceneManager.LoadScene("Menu Screen");
            showMenu = false;
        }
        startY += h + pad;

        if (GUI.Button(new Rect(startX, startY, w, h), "Close [L]"))
            showMenu = false;
    }
}
