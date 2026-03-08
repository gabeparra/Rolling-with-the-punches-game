using UnityEngine;
using UnityEngine.SceneManagement; // Required for switching scenes

public class MenuManager : MonoBehaviour
{
    // Make sure the name in quotes matches your Scene name EXACTLY
    public void PlayGame()
    {
        SceneManager.LoadScene("Hector Scene"); 
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("The player has quit the game!"); // Confirms it works in the Editor
    }
}