using UnityEngine;

[RequireComponent(typeof(ShopUI))]
public class ShopTrigger : MonoBehaviour
{
    private ShopUI shopUI;

    public void Awake()
    {
        shopUI = GetComponent<ShopUI>();
    }

    //TODO change to allow player to open shop while in trigger zone, rather than force-opening it
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            
            
            shopUI.Show();
        }
    }
}
