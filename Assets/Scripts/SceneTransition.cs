using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



/*HOW TO ADD MORE SCENES TO LIST
 1. add its short name to the enum (this is what shows in the dropdown in editor)
 2. add an entry to the dictionary, where the string is the name of the Scene file
 3. make sure the scene is added in the build profiles*/


public class SceneTransition : MonoBehaviour
{
    [SerializeField] private SceneName sceneName; //lets you choose from dropdown
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            string str = dict.GetValueOrDefault(sceneName, "HubScene");
            SceneManager.LoadScene(str);
        }
    }

    
}
