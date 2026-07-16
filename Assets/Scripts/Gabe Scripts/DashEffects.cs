using System.Collections;
using UnityEngine;

/// <summary>
/// Visual + cooldown sweeteners for the dash. Auto-attached by
/// TrainPlayerController in Start. Drives:
///   - A trail renderer that fires while IsDashing.
///   - A snappy FOV "punch" on the main camera at dash start.
///   - An expanding radial pulse at dash start (and brighter on perfect dodge).
///   - Brief slow-mo + dash cooldown refund on perfect dodge (called by
///     PlayerHealth when an EnemyBullet would have hit during a dash).
/// </summary>
[DisallowMultipleComponent]
public class DashEffects : MonoBehaviour
{
    private TrainPlayerController _controller;
    private TrailRenderer _trail;
    private Camera _cam;
    private float _baseFOV = 60f;
    private float _fovBoost = 0f;
    private bool  _wasDashing;
    private Material _ringMat;

    void Awake()
    {
        _controller = GetComponent<TrainPlayerController>();
        _cam = Camera.main;
        if (_cam != null) _baseFOV = _cam.fieldOfView;
        // Sprites/Default is in Always-Included Shaders for URP and built-in;
        // safe across both editor and WebGL.
        Shader s = Shader.Find("Sprites/Default");
        if (s == null) s = Shader.Find("Unlit/Color");
        _ringMat = new Material(s);
    }

    void Update()
    {
        // Re-acquire camera once if scene reload changed Camera.main.
        if (_cam == null) _cam = Camera.main;

        bool now = _controller != null && _controller.IsDashing;
        if      (now && !_wasDashing) StartDash();
        else if (!now &&  _wasDashing) EndDash();
        _wasDashing = now;

        // Decay FOV boost back to 0. Use unscaledDeltaTime so slow-mo doesn't
        // freeze the camera mid-pulse — the punch should feel snappy regardless.
        _fovBoost = Mathf.MoveTowards(_fovBoost, 0f, 30f * Time.unscaledDeltaTime);
        if (_cam != null) _cam.fieldOfView = _baseFOV + _fovBoost;
    }

    void StartDash()
    {
        EnsureTrail();
        _trail.emitting = true;
        _trail.Clear();
        _fovBoost = 8f;
        SpawnRingPulse(transform.position, new Color(1f, 0.85f, 0.2f, 0.85f), maxRadius: 1.6f, duration: 0.35f);
    }

    void EndDash()
    {
        if (_trail != null) _trail.emitting = false;
    }

    /// <summary>
    /// Fires when an enemy bullet would have hit during the dash. Refunds
    /// half the dash cooldown and plays a brighter, bigger pulse + brief
    /// slow-mo so dodging feels like a skill move, not a passive immunity.
    /// </summary>
    public void TriggerPerfectDodge()
    {
        SpawnRingPulse(transform.position, new Color(1f, 1f, 1f, 0.95f), maxRadius: 2.6f, duration: 0.45f);
        if (_controller != null) _controller.RefundDashCooldown(0.5f);
        StartCoroutine(SlowMo(0.5f, 0.15f));
    }

    void EnsureTrail()
    {
        if (_trail != null) return;
        var go = new GameObject("DashTrail");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        _trail = go.AddComponent<TrailRenderer>();
        _trail.material   = _ringMat;
        _trail.startColor = new Color(1f, 0.85f, 0.2f, 0.9f);
        _trail.endColor   = new Color(1f, 0.85f, 0.2f, 0f);
        _trail.startWidth = 0.6f;
        _trail.endWidth   = 0f;
        _trail.time       = 0.45f;
        _trail.minVertexDistance = 0.04f;
        _trail.emitting   = false;
        _trail.alignment  = LineAlignment.View;
    }

    void SpawnRingPulse(Vector3 worldPos, Color color, float maxRadius, float duration)
    {
        var go = new GameObject("DashPulse");
        go.transform.position = new Vector3(worldPos.x, worldPos.y + 0.05f, worldPos.z);
        var lr = go.AddComponent<LineRenderer>();
        lr.material        = _ringMat;
        lr.startColor      = color;
        lr.endColor        = color;
        lr.widthMultiplier = 0.08f;
        lr.useWorldSpace   = false;
        lr.loop            = true;
        const int segs = 36;
        lr.positionCount = segs;
        StartCoroutine(AnimatePulse(go, lr, segs, maxRadius, color, duration));
    }

    IEnumerator AnimatePulse(GameObject go, LineRenderer lr, int segs, float maxRadius, Color color, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // unscaled — visuals shouldn't slow during slow-mo
            float p = Mathf.Clamp01(t / duration);
            float r = Mathf.Lerp(0.1f, maxRadius, p);
            float a = (1f - p) * color.a;
            Color c = new Color(color.r, color.g, color.b, a);
            lr.startColor = c;
            lr.endColor   = c;
            for (int i = 0; i < segs; i++)
            {
                float angle = (i / (float)segs) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r));
            }
            yield return null;
        }
        Destroy(go);
    }

    IEnumerator SlowMo(float scale, float realDuration)
    {
        // If multiple dodges chain, don't compound — restore from the saved
        // value at the *start* of the first slow-mo only.
        float prev = Time.timeScale;
        if (prev < 0.99f) yield break; // already in slow-mo, skip
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(realDuration);
        Time.timeScale = 1f;
    }
}
