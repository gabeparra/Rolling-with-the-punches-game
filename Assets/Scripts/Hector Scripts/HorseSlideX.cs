using UnityEngine;

public class HorseSlideX : MonoBehaviour
{
    [Header("Tracking")]
    [Tooltip("Transform whose X this horse mirrors. Auto-finds the GameObject tagged 'Player' at Awake if left empty.")]
    public Transform follow;

    [Header("Lead Drift")]
    [Tooltip("Extra units per second the horse pulls ahead of the player in +X. 0 = locked alongside; 2-4 = a little faster than the train; higher = drifts off-screen ahead.")]
    public float leadSpeed = 2f;

    [Tooltip("Locks world Y and Z to the values at Awake so the horse can't drift vertically or sideways.")]
    public bool lockYZ = true;

    private float _offsetX;
    private float _lockedY;
    private float _lockedZ;

    void Awake()
    {
        if (follow == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) follow = go.transform;
        }

        _lockedY = transform.position.y;
        _lockedZ = transform.position.z;
        if (follow != null)
            _offsetX = transform.position.x - follow.position.x;
    }

    void Update()
    {
        if (follow == null) return;

        // Drift the offset further into+-X each frame so the horse leads the player/train.
        _offsetX += leadSpeed * Time.deltaTime;

        Vector3 p = transform.position;
        p.x = follow.position.x + _offsetX;
        if (lockYZ) { p.y = _lockedY; p.z = _lockedZ; }
        transform.position = p;
    }
}
