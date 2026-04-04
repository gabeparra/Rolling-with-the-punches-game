using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Health")]
    public Image[] heartIcons;

    [Header("Ammo")]
    public TextMeshProUGUI ammoText;
    public int maxAmmo = 6;

    [Header("Enemies")]
    public TextMeshProUGUI waveText;

    [Header("Gold")]
    public TextMeshProUGUI goldText;

    private int _hearts;
    private int _ammo;
    private int _gold = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _hearts = heartIcons != null ? heartIcons.Length : 0;
        _ammo = maxAmmo;
        DrawHearts();
        DrawAmmo();
        DrawGold();
        DrawEnemies();
    }

    public void LoseHeart()
    {
        if (_hearts <= 0) return;
        _hearts--;
        DrawHearts();
    }

    void DrawHearts()
    {
        if (heartIcons == null || heartIcons.Length == 0) return;
        for (int i = 0; i < heartIcons.Length; i++)
            if (heartIcons[i] != null)
                heartIcons[i].enabled = (i < _hearts);
    }

    public bool HasAmmo() => _ammo > 0;

    public void SpendAmmo()
    {
        if (_ammo > 0) _ammo--;
        DrawAmmo();
    }

    public void ReloadAmmo()
    {
        _ammo = maxAmmo;
        DrawAmmo();
    }

    void DrawAmmo()
    {
        if (ammoText != null) ammoText.text = _ammo + "/" + maxAmmo;
    }

    public void OnEnemyKilled()
    {
        _gold += 5;
        DrawGold();
        DrawEnemies();
    }

    public void DrawEnemies()
    {
        if (waveText == null) return;
        int count = FindObjectsByType<Enemy_AI>(FindObjectsSortMode.None).Length;
        waveText.text = "Enemies Remaining: " + count;
    }

    void DrawGold()
    {
        if (goldText != null) goldText.text = _gold.ToString();
    }
}