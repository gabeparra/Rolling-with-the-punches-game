using UnityEngine;

/// <summary>
/// Place this on a GameObject in the gameplay scene alongside all three environment spawners.
/// It enables only the spawner matching LevelSelector.CurrentTheme and disables the others.
/// Also swaps terrain material, skybox, fog, background tinting, and weather particles.
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

    [Header("Background Object (tinted for snow/mountain themes)")]
    public GameObject butteWallBG;

    [Header("Snow Fog")]
    [SerializeField] Color snowFogColor = new Color(0.82f, 0.88f, 0.95f, 1f);
    [SerializeField] float snowFogStart = 40f;
    [SerializeField] float snowFogEnd = 280f;

    [Header("Mountain Fog")]
    [SerializeField] Color mountainFogColor = new Color(0.75f, 0.72f, 0.68f, 1f);
    [SerializeField] float mountainFogStart = 50f;
    [SerializeField] float mountainFogEnd = 300f;

    [Header("Snowfall")]
    [SerializeField] bool enableSnowfall = true;
    [SerializeField] float snowAreaSize = 80f;
    [SerializeField] float snowHeight = 40f;
    [SerializeField] int maxSnowParticles = 3000;
    [SerializeField] float snowRate = 500f;
    [SerializeField] float snowFlakeSize = 0.15f;

    [Header("Mountain Wind & Dust")]
    [SerializeField] bool enableWind = true;
    [SerializeField] float windAreaSize = 100f;
    [SerializeField] float windHeight = 15f;
    [SerializeField] int maxDustParticles = 2000;
    [SerializeField] float dustRate = 300f;
    [SerializeField] float windStrength = 8f;

    void Awake()
    {
        // Auto-find references if not assigned in Inspector
        if (butteWallBG == null)
        {
            var bg = GameObject.Find("Butte Wall BG");
            if (bg != null) butteWallBG = bg;
        }
        if (groundRenderer == null)
        {
            var ground = GameObject.Find("Ground");
            if (ground != null) groundRenderer = ground.GetComponent<Renderer>();
        }

        if (westernSpawner != null) westernSpawner.SetActive(false);
        if (snowySpawner != null) snowySpawner.SetActive(false);
        if (mountainSpawner != null) mountainSpawner.SetActive(false);

        switch (LevelSelector.CurrentTheme)
        {
            case LevelSelector.Theme.Snowy:
                if (snowySpawner != null) snowySpawner.SetActive(true);
                ApplyTheme(snowyGroundMaterial, snowySkybox);
                ApplyFog(snowFogColor, snowFogStart, snowFogEnd);
                TintBackground(new Color(0.78f, 0.82f, 0.92f), 0.85f);
                if (enableSnowfall) CreateSnowfall();
                break;

            case LevelSelector.Theme.Mountain:
                if (mountainSpawner != null) mountainSpawner.SetActive(true);
                ApplyTheme(mountainGroundMaterial, mountainSkybox);
                ApplyFog(mountainFogColor, mountainFogStart, mountainFogEnd);
                if (enableWind) CreateWindDust();
                break;

            default:
                if (westernSpawner != null) westernSpawner.SetActive(true);
                ApplyTheme(westernGroundMaterial, westernSkybox);
                RenderSettings.fog = false;
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

    void ApplyFog(Color color, float start, float end)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = color;
        RenderSettings.fogStartDistance = start;
        RenderSettings.fogEndDistance = end;
    }

    void TintBackground(Color tint, float strength)
    {
        if (butteWallBG == null) return;

        foreach (var rend in butteWallBG.GetComponentsInChildren<Renderer>(true))
        {
            var mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", null);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", Color.Lerp(mat.GetColor("_BaseColor"), tint, strength));
                else if (mat.HasProperty("_Color"))
                    mat.color = Color.Lerp(mat.color, tint, strength);
            }
            rend.materials = mats;
        }
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
