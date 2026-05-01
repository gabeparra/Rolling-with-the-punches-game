using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*HOW TO ADD MORE SCENES TO LIST
 1. add its short name to the enum (this is what shows in the dropdown in editor)
 2. add an entry to the dictionary, where the string is the name of the Scene file
 3. make sure the scene is added in the build profiles*/

public class SceneTransition : MonoBehaviour
{
    [SerializeField][Tooltip("Control whether transition happens instantly or player must interact")]
    private bool instant = true;

    [SerializeField][Tooltip("The scene that will be transitioned to")]
    private SceneName sceneName;

    [SerializeField][Tooltip("Which procedural theme to use (only matters for gameplay levels)")]
    private LevelSelector.Theme theme = LevelSelector.Theme.Western;

    private bool canLoad = false;

    private enum SceneName
    {
        Hub,
        Western,
        Snow,
        Mountain,
        MainMenu
    }

    private readonly Dictionary<SceneName, string> dict = new Dictionary<SceneName, string>
    {
        {SceneName.Hub, "HubScene"},
        {SceneName.Western, "Hector Scene"},
        {SceneName.Snow, "Hector Scene"},
        {SceneName.Mountain, "Hector Scene"},
        {SceneName.MainMenu, "Menu Screen"}
    };

    public void OnEnable()
    {
        InputSystem.actions["Interact"].performed += OnInteract;
    }

    public void OnDisable()
    {
        InputSystem.actions["Interact"].performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (canLoad) Load();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (instant)
            {
                Load();
            }
            else
            {
                canLoad = true;
                HubManager.ShowPrompt("Start Trip");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canLoad = false;
            HubManager.HidePrompt();
        }
    }

    private void Load()
    {
        LevelSelector.CurrentTheme = theme;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        string str = dict.GetValueOrDefault(sceneName, "HubScene");
        SceneManager.LoadScene(str);
    }
}
