using System.Collections;
using UnityEngine;

// A short establishing sweep of the lounge before control begins: the camera
// orbits the centre tree, then hands off to the gameplay camera (FirstPersonCam
// if present, else CameraFollow). Any click after the grace period skips it.
public class IntroSweep : MonoBehaviour
{
    public Transform focus;            // the lounge centrepiece (set by setup; falls back by name)
    public float duration = 6.5f;
    public float orbitDistance = 8.5f;
    public float orbitHeight = 3.4f;
    public float yawFrom = 150f;
    public float yawTo = 55f;

    Behaviour follow;        // the gameplay camera controller to enable afterwards
    CanvasGroup titleCard;   // optional title overlay

    void Awake()
    {
        follow = GetComponent<FirstPersonCam>();
        if (follow == null) follow = GetComponent<CameraFollow>();
        var card = GameObject.Find("/Canvas/TitleCard");
        if (card != null) titleCard = card.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (focus == null)
        {
            var centre = GameObject.Find("/Lounge/TreeCentre");
            if (centre != null) focus = centre.transform;
        }
        if (focus == null) { enabled = false; return; }
        StartCoroutine(Sweep());
    }

    IEnumerator Sweep()
    {
        follow.enabled = false;
        float t = 0f;
        // Use renderer bounds, not the transform: several scene groups keep
        // their pivot at the world origin with children at world coordinates.
        Vector3 centre = focus.position;
        var rends = focus.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            centre = new Vector3(b.center.x, b.min.y, b.center.z);
        }
        Vector3 pivot = centre + Vector3.up * 1.3f;
        while (t < duration)
        {
            // skip on tap — but not in the first moments, so the tap that
            // launched the game doesn't instantly cancel the sweep
            if (t > 0.9f && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) break;
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            float yaw = Mathf.Lerp(yawFrom, yawTo, k) * Mathf.Deg2Rad;
            Vector3 pos = pivot + new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw)) * orbitDistance;
            pos.y = pivot.y + orbitHeight;
            transform.position = pos;
            transform.LookAt(pivot);
            if (titleCard != null)
            {
                float fadeIn = Mathf.InverseLerp(0.4f, 1.6f, t);
                float fadeOut = 1f - Mathf.InverseLerp(duration - 1.6f, duration - 0.2f, t);
                titleCard.alpha = Mathf.Min(fadeIn, fadeOut);
            }
            yield return null;
        }
        if (titleCard != null) titleCard.alpha = 0f;
        follow.enabled = true;   // CameraFollow lerps from here to its own pose
        Destroy(this);
    }
}
