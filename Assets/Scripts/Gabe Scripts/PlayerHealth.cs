using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Local Y below which the player is considered fallen off the train")]
    public float fallThreshold = 0.5f;

    // BUG-26 fix: prevent double scene-reload from fall + death in same frame
    private static bool _isReloading = false;
    private AudioClip _oofClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => _isReloading = false;

    void OnEnable() => _isReloading = false;

    void Awake()
    {
        _oofClip = GenerateOofSound();
    }

    /// <summary>Generates a short descending tone similar to the classic Roblox "oof".</summary>
    static AudioClip GenerateOofSound()
    {
        int sampleRate = 44100;
        float duration = 0.25f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;
            // Descending frequency from ~400Hz to ~200Hz
            float freq = Mathf.Lerp(400f, 200f, progress);
            float amplitude = 1f - progress; // fade out
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amplitude * 0.6f;
        }

        AudioClip clip = AudioClip.Create("OofSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void Update()
    {
        if (_isReloading) return;
        if (transform.localPosition.y < fallThreshold)
        {
            _isReloading = true;
            int gold = HUDManager.Instance != null ? HUDManager.Instance.Gold : 0;
            int kills = HUDManager.Instance != null ? HUDManager.Instance.TotalKills : 0;

            if (GameUIManager.Instance != null)
                GameUIManager.Instance.ShowGameOver(gold, kills);
            else
                SceneManager.LoadScene("HubScene");
        }
    }

    public void TakeDamage(int amount)
    {
        if (_oofClip != null)
            AudioSource.PlayClipAtPoint(_oofClip, transform.position);

        for (int i = 0; i < amount; i++)
            if (HUDManager.Instance != null)
                HUDManager.Instance.LoseHeart();
    }

    // BUG-09 fix: deactivate bullet immediately to prevent double-damage
    // from both collision + trigger callbacks firing in the same frame
    void HandleEnemyBullet(GameObject bulletObj)
    {
        if (bulletObj == null || !bulletObj.activeSelf) return;
        bulletObj.SetActive(false); // blocks other callbacks this frame
        TakeDamage(1);
        Destroy(bulletObj);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyBullet"))
            HandleEnemyBullet(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.CompareTag("EnemyBullet"))
            HandleEnemyBullet(other.gameObject);
    }
}