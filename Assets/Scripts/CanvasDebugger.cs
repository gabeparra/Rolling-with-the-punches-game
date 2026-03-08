using UnityEngine;
using UnityEngine.EventSystems;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Mouse is over UI!");
            }
            else
            {
                Debug.Log("Mouse is NOT over UI.");
            }
        }
    }
}
