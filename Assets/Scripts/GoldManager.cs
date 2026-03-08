using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    private int currentGold = 0;

    void Start()
    {
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Formats as "GOLD: 00050"
        goldText.text = "GOLD: " + currentGold.ToString("D5");
    }
}