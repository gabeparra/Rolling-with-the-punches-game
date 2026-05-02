using UnityEngine;

/// <summary>
/// Continuously adjusts localScale so the object maintains a uniform
/// world scale regardless of parent rotation/scale changes.
/// Attach to any child of a non-uniformly-scaled, rotating parent.
/// </summary>
public class MaintainWorldScale : MonoBehaviour
{
    public float targetScale = 0.75f;

    void LateUpdate()
    {
        // Set unit scale first so lossyScale reflects only the parent chain
        // (including this object's rotation under the non-uniform parent).
        transform.localScale = Vector3.one;
        Vector3 lossy = transform.lossyScale;

        float lx = Mathf.Abs(lossy.x) > 0.001f ? Mathf.Abs(lossy.x) : 1f;
        float ly = Mathf.Abs(lossy.y) > 0.001f ? Mathf.Abs(lossy.y) : 1f;
        float lz = Mathf.Abs(lossy.z) > 0.001f ? Mathf.Abs(lossy.z) : 1f;

        transform.localScale = new Vector3(
            targetScale / lx,
            targetScale / ly,
            targetScale / lz
        );
    }
}
