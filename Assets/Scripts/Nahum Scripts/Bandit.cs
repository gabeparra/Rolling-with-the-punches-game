using UnityEngine;

public class Bandit : MonoBehaviour
{
    public float accuracy = .3f;

    public float shoot_force = 8590f;

    public const int max_mag_size = 6;
    int mag_size = max_mag_size;

    public float shoot_interval = .75f;

    bool reloading = false;

    public float reload_time = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
