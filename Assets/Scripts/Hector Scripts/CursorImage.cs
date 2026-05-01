using UnityEngine;
/*
    The point of this script is to change the cursor image to a gun crosshair for aesthetics
    May remove this feature later based on the results of player feedback 
*/
public class CursorImage : MonoBehaviour
{
    public Texture2D cursorTexture; // This is to store the image data for the sprite
    public Vector2 hotspot = Vector2.zero; // Helps to calibrate the cursor image on screen
    public bool autoCenterHotspot = false;

    void Start()
    {
        if(autoCenterHotspot) hotspot = new Vector2(cursorTexture.width/2, cursorTexture.height/2);
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto); // Binds the image, hotspot, and sets cursor to automode.
        Cursor.visible = true; // Toggles if cursor is visual or not -- good for testing.
    }
}
