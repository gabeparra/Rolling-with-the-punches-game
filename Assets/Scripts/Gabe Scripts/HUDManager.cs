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

    [Header("Enemies")]
    public TextMeshProUGUI waveText;
    [Tooltip("Set from spawner or Inspector. Defaults to 8 to match FloorEnemySpawnerSpawner.")]
    public int totalEnemies = 8;

    [Header("Gold")]
    public TextMeshProUGUI goldText;

    private int _ammo;
    private int _enemiesRemaining;
    private int _totalKills = 0;

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
        _ammo = PlayerStats.magSize;
        _enemiesRemaining = totalEnemies;

        DrawAll();
    }
    
    public void DrawAll()
    {
        DrawHearts();
        DrawAmmo();
        DrawGold();
        DrawEnemies();
    }

    public void LoseHeart()
    {
        if (GameManager.getPlayerHealth() <= 0) return;
        GameManager.UpdatePlayerHealth(-1);

        // Losing a heart permanently reduces max ammo by 1
        /*
        _maxAmmo = Mathf.Max(1, _maxAmmo - 1);
        if (_ammo > _maxAmmo) _ammo = _maxAmmo;
        */

        DrawHearts();
        //DrawAmmo();

        if (GameManager.getPlayerHealth() <= 0)
        {
            // BUG-26 fix: guard against double scene reload
            if (_isReloading) return;
            _isReloading = true;
            if (GameUIManager.Instance != null)
                GameUIManager.Instance.ShowGameOver(GameManager.getCurrency(), _totalKills);
            else
                SceneManager.LoadScene("HubScene");
        }
    }

    void DrawHearts()
    {
        if (heartIcons == null || heartIcons.Length == 0) return;
        for (int i = 0; i < heartIcons.Length; i++)
            if (heartIcons[i] != null)
                heartIcons[i].enabled = (i < GameManager.getPlayerHealth());
    }

    public bool HasAmmo() => _ammo > 0;

    public void SpendAmmo()
    {
        if (_ammo > 0) _ammo--;
        DrawAmmo();
    }

    public void ReloadAmmo()
    {
        _ammo = PlayerStats.magSize;
        DrawAmmo();
    }

    void DrawAmmo()
    {
        if (ammoText != null) ammoText.text = _ammo + "/" + PlayerStats.magSize;
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
        GameManager.UpdateCurrency(PlayerStats.goldPerKill);
        _totalKills++;
        _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);

        DrawGold();
        DrawEnemies();

        if (_enemiesRemaining <= 0 && GameUIManager.Instance != null)
            GameUIManager.Instance.ShowWin(GameManager.getCurrency(), _totalKills);
    }

    /// <summary>Bandit escaped with stolen gold — counts toward remaining but no kill reward.</summary>
    public void OnEnemyEscaped()
    {
        _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);
        DrawEnemies();
        if (_enemiesRemaining <= 0 && GameUIManager.Instance != null)
            GameUIManager.Instance.ShowWin(GameManager.getCurrency(), _totalKills);
    }

    /// <summary>Subtract gold from the player (clamped at 0). Used when bandits escape with loot.</summary>
    public void LoseGold(int amount)
    {
        if (amount <= 0) return;
        GameManager.UpdateCurrency(-amount);
        DrawGold();
    }

    public void DrawEnemies()
    {
        if (waveText == null) return;
        waveText.text = "Enemies Remaining: " + _enemiesRemaining;
    }

    void DrawGold()
    {
        if (goldText != null) goldText.text = GameManager.getCurrency().ToString();
    }
}