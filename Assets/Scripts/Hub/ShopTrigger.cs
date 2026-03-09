using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [SerializeField]
    private ShopUI shopUI;

    //TODO change to allow player to open shop while in trigger zone, rather than force-opening it
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            shopUI.Show();
        }
    }
}
