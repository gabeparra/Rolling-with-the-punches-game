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
    // whether the player is allowed to transition to the next scene
    private bool canLoad = false;
    [SerializeField][Tooltip("The scene that will be transitioned to")]
    private SceneName sceneName; //lets you choose from dropdown
    private enum SceneName //what shows in the dropdown
    {
        Hub,
        Town1,
        Town2
    }

    //connect the choice to its actual file name
    private readonly Dictionary<SceneName, string> dict = new Dictionary<SceneName, string>
    {
        {SceneName.Hub, "HubScene"},
        {SceneName.Town1, "Town1"}
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
        Debug.Log("interact press seen by " + gameObject);
        if(canLoad) Load();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(instant) Load();
            else 
            {
                canLoad = true;
                Debug.Log("canLoad = true");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            canLoad = false;
            Debug.Log("canLoad = false");
        }
    }

    private void Load()
    {
        Debug.Log("Loading scene: " + sceneName);
        string str = dict.GetValueOrDefault(sceneName, "HubScene");
        SceneManager.LoadScene(str);
    }

    
}
