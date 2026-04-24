using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Health")]
    public Image[] heartIcons;

    [Header("Ammo")]
    public TextMeshProUGUI ammoText;
    public int startingAmmo = 6;

    [Header("Enemies")]
    public TextMeshProUGUI waveText;
    [Tooltip("Set from spawner or Inspector. Defaults to 8 to match FloorEnemySpawnerSpawner.")]
    public int totalEnemies = 8;

    [Header("Gold")]
    public TextMeshProUGUI goldText;

    private int _hearts;
    private int _ammo;
    private int _maxAmmo;
    private int _gold = 0;
    private int _enemiesRemaining;
    private int _totalKills = 0;

    public int Gold => _gold;
    public int TotalKills => _totalKills;

    // BUG-26 fix: shared reload guard with PlayerHealth
    private static bool _isReloading = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => _isReloading = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _isReloading = false;
    }

    void Start()
    {
        _hearts = heartIcons != null ? heartIcons.Length : 0;
        _maxAmmo = startingAmmo;
        _ammo = _maxAmmo;
        _enemiesRemaining = totalEnemies;

        // Load persistent gold from GameManager
        if (GameManager.Instance != null)
            _gold = GameManager.Instance.Gold;

        DrawHearts();
        DrawAmmo();
        DrawGold();
        DrawEnemies();
    }

    public void LoseHeart()
    {
        if (_hearts <= 0) return;
        _hearts--;

        // Losing a heart permanently reduces max ammo by 1
        _maxAmmo = Mathf.Max(1, _maxAmmo - 1);
        if (_ammo > _maxAmmo) _ammo = _maxAmmo;

        DrawHearts();
        DrawAmmo();

        if (_hearts <= 0)
        {
            // BUG-26 fix: guard against double scene reload
            if (_isReloading) return;
            _isReloading = true;
            if (GameUIManager.Instance != null)
                GameUIManager.Instance.ShowGameOver(_gold, _totalKills);
            else
                SceneManager.LoadScene("HubScene");
        }
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
        _ammo = _maxAmmo;
        DrawAmmo();
    }

    void DrawAmmo()
    {
        if (ammoText != null) ammoText.text = _ammo + "/" + _maxAmmo;
    }

    /// <summary>Set the total enemy count (call from spawner at runtime).</summary>
    public void SetTotalEnemies(int count)
    {
        totalEnemies = count;
        _enemiesRemaining = count;
        DrawEnemies();
    }

    public void OnEnemyKilled()
    {
        _gold += 5;
        _totalKills++;
        _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);

        // Persist gold immediately so it survives death/win
        if (GameManager.Instance != null)
            GameManager.Instance.AddGold(5);

        DrawGold();
        DrawEnemies();

        if (_enemiesRemaining <= 0 && GameUIManager.Instance != null)
            GameUIManager.Instance.ShowWin(_gold, _totalKills);
    }

    public void DrawEnemies()
    {
        if (waveText == null) return;
        waveText.text = "Enemies Remaining: " + _enemiesRemaining;
    }

    void DrawGold()
    {
        if (goldText != null) goldText.text = _gold.ToString();
    }
}