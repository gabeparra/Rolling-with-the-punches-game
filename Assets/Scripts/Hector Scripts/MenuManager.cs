using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        LevelSelector.CurrentTheme = LevelSelector.Theme.Western;
        SceneManager.LoadScene("Hector Scene");
    }

    public void PlaySnowyScene()
    {
        LevelSelector.CurrentTheme = LevelSelector.Theme.Snowy;
        SceneManager.LoadScene("Snow scene");
    }

    public void PlayMountainScene()
    {
        LevelSelector.CurrentTheme = LevelSelector.Theme.Mountain;
        SceneManager.LoadScene("Mountain scene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("The player has quit the game!");
    }
}