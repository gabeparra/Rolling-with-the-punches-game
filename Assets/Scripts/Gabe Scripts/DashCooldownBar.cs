using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space canvas dash cooldown bar that follows the player.
/// </summary>
public class DashCooldownBar : MonoBehaviour
{
    private TrainPlayerController _player;
    private Canvas _canvas;
    private Image _fillImage;
    private bool _ready = true;
    private Camera _cam;

    void Start()
    {
        _player = GetComponent<TrainPlayerController>();
        if (_player == null) { enabled = false; return; }
        _cam = Camera.main;

        // World-space canvas
        var canvasGO = new GameObject("DashBarCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        var rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 10);
        rt.localScale = Vector3.one * 0.005f;

        // Background
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        // Fill
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(canvasGO.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0);
        fillRT.anchorMax = new Vector2(1, 1);
        fillRT.offsetMin = new Vector2(2, 2);
        fillRT.offsetMax = new Vector2(-2, -2);
        fillRT.pivot = new Vector2(0, 0.5f);
        _fillImage = fillGO.AddComponent<Image>();
        _fillImage.color = Color.black;

        _canvas.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (_player == null || _canvas == null) return;

        float t = _player.DashCooldownNormalized;

        if (t <= 0f)
        {
            if (!_ready)
            {
                _ready = true;
                _canvas.gameObject.SetActive(false);
            }
            return;
        }

        if (_ready)
        {
            _ready = false;
            _canvas.gameObject.SetActive(true);
        }

        // Position below player feet, face camera
        Vector3 pos = transform.position + Vector3.down * 0.15f;
        _canvas.transform.position = pos;
        if (_cam != null)
            _canvas.transform.rotation = _cam.transform.rotation;

        // Fill from left: scale X by progress
        float fill = 1f - t;
        var fillRT = _fillImage.rectTransform;
        fillRT.anchorMax = new Vector2(fill, 1);
        fillRT.offsetMax = new Vector2(-2, -2);
    }

    void OnDestroy()
    {
        if (_canvas != null) Destroy(_canvas.gameObject);
    }
}
