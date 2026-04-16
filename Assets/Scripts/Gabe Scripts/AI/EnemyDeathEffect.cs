using UnityEngine;

public class EnemyDeathEffect : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float effectLifetime = 2f;

    private bool _effectsPlayed = false;
    private static AudioClip _oofClip;

    /// <summary>
    /// Call this BEFORE Destroy() to spawn death VFX and sound safely.
    /// </summary>
    public void PlayDeathEffects()
    {
        if (_effectsPlayed) return;
        _effectsPlayed = true;

        Vector3 pos = transform.position;

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, pos, Quaternion.identity);
            Destroy(fx, effectLifetime);
        }

        PlayOof(pos);
    }

    /// <summary>
    /// Plays a procedural Roblox-style "oof" sound at the given position.
    /// Static so EnemyScript and other death paths can reuse it.
    /// </summary>
    public static void PlayOof(Vector3 pos)
    {
        if (_oofClip == null)
            _oofClip = GenerateOofClip();

        GameObject tempGO = new GameObject("OofSound");
        tempGO.transform.position = pos;
        AudioSource src = tempGO.AddComponent<AudioSource>();
        src.clip = _oofClip;
        src.spatialBlend = 0f;
        src.volume = 0.8f;
        src.pitch = Random.Range(0.9f, 1.1f);
        src.Play();
        Destroy(tempGO, _oofClip.length + 0.1f);
    }

    private static AudioClip GenerateOofClip()
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Roblox oof: a short vocal-like tone that drops in pitch
        // Two sine layers for a fuller "oof" sound
        float startFreq = 480f;
        float endFreq = 200f;
        float phase1 = 0f;
        float phase2 = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;

            // Frequency sweeps down quickly then levels off
            float curve = 1f - Mathf.Pow(1f - t, 3f); // fast drop
            float freq = Mathf.Lerp(startFreq, endFreq, curve);

            // Amplitude envelope: quick attack, sustain, then fade
            float env;
            if (t < 0.05f)
                env = t / 0.05f; // 50ms attack
            else if (t < 0.6f)
                env = 1f; // sustain
            else
                env = 1f - (t - 0.6f) / 0.4f; // fade out

            // Two harmonics for a richer tone
            float dt = 1f / sampleRate;
            phase1 += freq * dt;
            phase2 += freq * 1.5f * dt; // 3rd harmonic at lower volume

            float sample = Mathf.Sin(phase1 * 2f * Mathf.PI) * 0.7f
                         + Mathf.Sin(phase2 * 2f * Mathf.PI) * 0.2f;

            samples[i] = sample * env * 0.9f;
        }

        AudioClip clip = AudioClip.Create("Oof", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
