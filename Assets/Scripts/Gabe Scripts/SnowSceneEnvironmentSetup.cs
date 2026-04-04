using UnityEngine;
using UnityEngine.Rendering;

public class SnowSceneEnvironmentSetup : MonoBehaviour
{
    [SerializeField] GameObject westernEnvironmentSpawner;
    [SerializeField] MeshRenderer groundRenderer;
    [SerializeField] Material snowGroundMaterial;
    [SerializeField] Material snowSkyboxMaterial;

    [Header("Atmosphere")]
    [SerializeField] bool configureFog = true;
    [SerializeField] Color fogColor = new Color(0.82f, 0.88f, 0.95f, 1f);
    [SerializeField] float fogStartDistance = 40f;
    [SerializeField] float fogEndDistance = 280f;

    [Header("Snowfall")]
    [SerializeField] bool enableSnowfall = true;
    [SerializeField] float snowAreaSize = 80f;
    [SerializeField] float snowHeight = 40f;
    [SerializeField] int maxSnowParticles = 3000;
    [SerializeField] float snowRate = 500f;
    [SerializeField] float snowFlakeSize = 0.15f;

    void Awake()
    {
        LevelSelector.CurrentTheme = LevelSelector.Theme.Snowy;

        if (westernEnvironmentSpawner != null)
            westernEnvironmentSpawner.SetActive(false);

        if (snowGroundMaterial == null)
            snowGroundMaterial = CreateFallbackMaterial(new Color(0.92f, 0.93f, 0.96f));

        if (groundRenderer != null)
            groundRenderer.material = snowGroundMaterial;

        if (snowSkyboxMaterial != null)
            RenderSettings.skybox = snowSkyboxMaterial;

        if (configureFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }

        if (enableSnowfall)
            CreateSnowfall();
    }

    void CreateSnowfall()
    {
        var snowGO = new GameObject("Snowfall");
        snowGO.transform.SetParent(transform);
        snowGO.transform.localPosition = new Vector3(0f, snowHeight, 0f);

        var ps = snowGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.maxParticles = maxSnowParticles;
        main.startLifetime = snowHeight / 2f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(snowFlakeSize * 0.5f, snowFlakeSize);
        main.startColor = new Color(0.95f, 0.95f, 1f, 0.8f);
        main.gravityModifier = 0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = snowRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(snowAreaSize, 1f, snowAreaSize);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.2f;

        var renderer = snowGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial(new Color(0.95f, 0.95f, 1f, 0.7f));
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
