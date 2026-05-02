using UnityEngine;

public static class EnemyDeathFX
{
    public static void Play(Vector3 position, Color tint)
    {
        GameObject outer = new GameObject("DeathFX_Burst");
        outer.transform.position = position;

        ParticleSystem ps = outer.AddComponent<ParticleSystem>();
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 36) });

        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startColor = tint;
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(tint, 0.3f),
                new GradientColorKey(tint * 0.4f, 1f),
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f),
            });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0.2f));
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer rend = outer.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));

        ps.Play();
        Object.Destroy(outer, 1.5f);

        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "DeathFX_Flash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.4f;
        Object.Destroy(flash.GetComponent<Collider>());
        Renderer fr = flash.GetComponent<Renderer>();
        Material flashMat = new Material(Shader.Find("Sprites/Default"));
        flashMat.color = new Color(1f, 1f, 0.9f, 0.85f);
        fr.material = flashMat;
        flash.AddComponent<DeathFlash>().tint = tint;
    }

    private class DeathFlash : MonoBehaviour
    {
        public Color tint;
        const float duration = 0.18f;
        float t;
        Renderer rend;

        void Start() { rend = GetComponent<Renderer>(); }

        void Update()
        {
            t += Time.deltaTime;
            float k = t / duration;
            transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.6f, k);
            if (rend != null)
            {
                Color c = Color.Lerp(new Color(1f, 1f, 0.9f, 0.85f), new Color(tint.r, tint.g, tint.b, 0f), k);
                rend.material.color = c;
            }
            if (t >= duration) Destroy(gameObject);
        }
    }
}
