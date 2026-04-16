using UnityEngine;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    private Enemy_AI _enemyAI;
    private int _maxHealth;
    private TextMeshPro _text;
    private Transform _cam;

    void Start()
    {
        _enemyAI = GetComponentInParent<Enemy_AI>();
        if (_enemyAI == null) { Destroy(this); return; }

        _maxHealth = _enemyAI.health;

        // Create floating text below the enemy
        var go = new GameObject("HPText");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, 2.5f, 0);

        _text = go.AddComponent<TextMeshPro>();
        _text.fontSize = 3f;
        _text.alignment = TextAlignmentOptions.Center;
        _text.sortingOrder = 100;
        _text.color = Color.red;

        // Size the text rect
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2f, 0.5f);

        _cam = Camera.main != null ? Camera.main.transform : null;

        UpdateDisplay();
    }

    void LateUpdate()
    {
        if (_enemyAI == null) { Destroy(gameObject); return; }

        UpdateDisplay();

        // Face camera
        if (_cam != null && _text != null)
            _text.transform.rotation = Quaternion.LookRotation(_text.transform.position - _cam.position);
    }

    void UpdateDisplay()
    {
        if (_text == null || _enemyAI == null) return;
        int hp = _enemyAI.health;
        _text.text = $"{hp}/{_maxHealth}";
        _text.color = hp > _maxHealth / 2 ? Color.green : hp > 1 ? Color.yellow : Color.red;
    }
}
