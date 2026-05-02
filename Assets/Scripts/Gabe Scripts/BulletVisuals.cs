using UnityEngine;

public static class BulletVisuals
{
    public static void Enhance(GameObject bullet, Color tint, float scale = 1.8f, bool trail = true)
    {
        if (bullet == null) return;

        bullet.transform.localScale *= scale;

        Renderer rend = bullet.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            Material mat = sh != null ? new Material(sh) : new Material(rend.sharedMaterial);
            mat.color = tint;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", tint * 4f);
                mat.EnableKeyword("_EMISSION");
            }
            rend.material = mat;
        }

        if (trail)
        {
            TrailRenderer tr = bullet.GetComponent<TrailRenderer>();
            if (tr == null) tr = bullet.AddComponent<TrailRenderer>();
            tr.time = 0.18f;
            tr.startWidth = 0.18f;
            tr.endWidth = 0.0f;
            tr.minVertexDistance = 0.05f;
            tr.numCornerVertices = 2;
            tr.numCapVertices = 2;
            tr.material = new Material(Shader.Find("Sprites/Default"));
            tr.startColor = tint;
            Color end = tint; end.a = 0f;
            tr.endColor = end;
        }
    }
}
