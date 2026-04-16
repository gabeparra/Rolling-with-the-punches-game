using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject startButton;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(startButton);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("HubScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("The player has quit the game!");
    }
}
