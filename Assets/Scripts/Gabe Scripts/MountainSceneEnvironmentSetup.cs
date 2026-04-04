using UnityEngine;
using UnityEngine.Rendering;

public class MountainSceneEnvironmentSetup : MonoBehaviour
{
    [SerializeField] GameObject westernEnvironmentSpawner;
    [SerializeField] MeshRenderer groundRenderer;
    [SerializeField] Material mountainGroundMaterial;
    [SerializeField] Material mountainSkyboxMaterial;

    [Header("Atmosphere")]
    [SerializeField] bool configureFog = true;
    [SerializeField] Color fogColor = new Color(0.75f, 0.72f, 0.68f, 1f);
    [SerializeField] float fogStartDistance = 50f;
    [SerializeField] float fogEndDistance = 300f;

    [Header("Wind & Dust")]
    [SerializeField] bool enableWind = true;
    [SerializeField] float windAreaSize = 100f;
    [SerializeField] float windHeight = 15f;
    [SerializeField] int maxDustParticles = 2000;
    [SerializeField] float dustRate = 300f;
    [SerializeField] float windStrength = 8f;

    void Awake()
    {
        LevelSelector.CurrentTheme = LevelSelector.Theme.Mountain;

        if (westernEnvironmentSpawner != null)
            westernEnvironmentSpawner.SetActive(false);

        if (mountainGroundMaterial == null)
            mountainGroundMaterial = CreateFallbackMaterial(new Color(0.45f, 0.38f, 0.32f));

        if (groundRenderer != null)
            groundRenderer.material = mountainGroundMaterial;

        if (mountainSkyboxMaterial != null)
            RenderSettings.skybox = mountainSkyboxMaterial;

        if (configureFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }

        if (enableWind)
            CreateWindDust();
    }

    void CreateWindDust()
    {
        var windGO = new GameObject("WindDust");
        windGO.transform.SetParent(transform);
        windGO.transform.localPosition = new Vector3(0f, windHeight * 0.5f, 0f);

        var ps = windGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.maxParticles = maxDustParticles;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startColor = new Color(0.65f, 0.55f, 0.45f, 0.3f);
        main.gravityModifier = -0.02f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = dustRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(windAreaSize, windHeight, windAreaSize);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(windStrength * 0.6f, windStrength);
        vel.y = new ParticleSystem.MinMaxCurve(-0.5f, 1.5f);
        vel.z = new ParticleSystem.MinMaxCurve(-1f, 2f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 1.5f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.8f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.65f, 0.55f, 0.45f), 0f), new GradientColorKey(new Color(0.65f, 0.55f, 0.45f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.35f, 0.2f), new GradientAlphaKey(0.35f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;

        var renderer = windGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial(new Color(0.65f, 0.55f, 0.45f, 0.3f));
    }

    static Material CreateFallbackMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    static Material CreateParticleMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        var mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_Blend", 0);
        return mat;
    }
}
