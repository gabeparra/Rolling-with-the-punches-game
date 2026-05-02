using UnityEngine;

public class DebugCollisionVisualizer : MonoBehaviour
{
    public static bool ShowDebug { get; private set; } = false;
    public KeyCode toggleKey = KeyCode.F2;

    private static DebugCollisionVisualizer _instance;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ShowDebug = !ShowDebug;
            Debug.Log($"[Debug] Collision visualization: {(ShowDebug ? "ON" : "OFF")}");
        }
    }

    void OnGUI()
    {
        if (!ShowDebug) return;

        // Show status label
        GUI.color = Color.green;
        GUI.Label(new Rect(10, 10, 300, 25), "COLLISION DEBUG ON (F2 to toggle)");

        // Find all active bullets and draw their info
        var bullets = FindObjectsByType<BulletTracer>(FindObjectsSortMode.None);
        Camera cam = Camera.main;
        if (cam == null) return;

        GUI.color = Color.yellow;
        foreach (var b in bullets)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(b.transform.position);
            if (screenPos.z > 0)
            {
                float y = Screen.height - screenPos.y;
                string tag = b.CompareTag("EnemyBullet") ? "ENEMY" : "PLAYER";
                GUI.Label(new Rect(screenPos.x - 30, y - 20, 120, 20), $"[{tag}]");
            }
        }
    }

    // Draw collider gizmos for all bullets when debug is on
    void OnDrawGizmos()
    {
        if (!ShowDebug) return;
        DrawBulletGizmos();
    }

    static void DrawBulletGizmos()
    {
        var bullets = FindObjectsByType<BulletTracer>(FindObjectsSortMode.None);
        foreach (var b in bullets)
        {
            // Draw collider bounds
            var col = b.GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = b.CompareTag("EnemyBullet") ? Color.red : Color.cyan;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                // No collider — draw a small sphere at position
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(b.transform.position, 0.15f);
            }

            // Draw ray forward showing bullet direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(b.transform.position, b.transform.forward * 2f);
        }
    }
}
