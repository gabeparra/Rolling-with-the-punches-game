using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject controlsPanel;

    //cursor image merged into this file
    public Texture2D cursorTexture; // This is to store the image data for the sprite
    public Vector2 hotspot = Vector2.zero; // Helps to calibrate the cursor image on screen
    public bool autoCenterHotspot = false;

    void Start()
    {

        SetCursor();
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

    public void ShowControls()
    {
        if(controlsPanel != null) controlsPanel.SetActive(true);
        if(mainPanel != null) mainPanel.SetActive(false);
        ResetCursor();
    }

    public void HideControls()
    {
        if(controlsPanel != null) controlsPanel.SetActive(false);
        if(mainPanel != null) mainPanel.SetActive(true);
        SetCursor();
    }

    public void SetCursor()
    {
        //taken from CursorImage.cs
        if(autoCenterHotspot) hotspot = new Vector2(cursorTexture.width/2, cursorTexture.height/2);
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto); // Binds the image, hotspot, and sets cursor to automode.
        Cursor.visible = true; // Toggles if cursor is visual or not -- good for testing.
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
    }
}
