using UnityEngine;

/// <summary>
/// Place this on a GameObject in the gameplay scene alongside all three environment spawners.
/// It enables only the spawner matching LevelSelector.CurrentTheme and disables the others.
/// Also swaps terrain material and skybox.
/// </summary>
public class ThemeLoader : MonoBehaviour
{
    [Header("Assign the spawner GameObjects (disabled by default in scene)")]
    public GameObject westernSpawner;
    public GameObject snowySpawner;
    public GameObject mountainSpawner;

    [Header("Ground Renderer to swap material on")]
    public Renderer groundRenderer;

    [Header("Ground Materials (one per theme)")]
    public Material westernGroundMaterial;
    public Material snowyGroundMaterial;
    public Material mountainGroundMaterial;

    [Header("Skybox Materials (one per theme, optional)")]
    public Material westernSkybox;
    public Material snowySkybox;
    public Material mountainSkybox;

    void Awake()
    {
        if (westernSpawner != null) westernSpawner.SetActive(false);
        if (snowySpawner != null) snowySpawner.SetActive(false);
        if (mountainSpawner != null) mountainSpawner.SetActive(false);

        switch (LevelSelector.CurrentTheme)
        {
            case LevelSelector.Theme.Snowy:
                if (snowySpawner != null) snowySpawner.SetActive(true);
                ApplyTheme(snowyGroundMaterial, snowySkybox);
                break;
            case LevelSelector.Theme.Mountain:
                if (mountainSpawner != null) mountainSpawner.SetActive(true);
                ApplyTheme(mountainGroundMaterial, mountainSkybox);
                break;
            default:
                if (westernSpawner != null) westernSpawner.SetActive(true);
                ApplyTheme(westernGroundMaterial, westernSkybox);
                break;
        }
    }

    void ApplyTheme(Material groundMat, Material skyboxMat)
    {
        if (groundRenderer != null && groundMat != null)
            groundRenderer.material = groundMat;

        if (skyboxMat != null)
            RenderSettings.skybox = skyboxMat;
    }
}
