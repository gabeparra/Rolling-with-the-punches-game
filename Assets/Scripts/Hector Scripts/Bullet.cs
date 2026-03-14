using UnityEngine;

public class Bullet : MonoBehaviour 
{
    public int damage;

    void OnCollisionEnter(Collision collision)
    {
        // Add a small sparks effect here later!
        Destroy(gameObject);
    } 
}