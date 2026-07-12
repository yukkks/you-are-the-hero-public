using System.Collections;
using UnityEngine;

// A short establishing sweep of the lounge before control begins: the camera
// orbits the centre tree, then hands off to CameraFollow. Any click skips it.
[RequireComponent(typeof(CameraFollow))]
public class IntroSweep : MonoBehaviour
{
    public Transform focus;            // the lounge centrepiece (set by setup; falls back by name)
    public float duration = 6.5f;
    public float orbitDistance = 8.5f;
    public float orbitHeight = 3.4f;
    public float yawFrom = 150f;
    public float yawTo = 55f;

    CameraFollow follow;

    void Awake()
    {
        follow = GetComponent<CameraFollow>();
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
        Vector3 pivot = focus.position + Vector3.up * 1.1f;
        while (t < duration)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) break;   // skip
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            float yaw = Mathf.Lerp(yawFrom, yawTo, k) * Mathf.Deg2Rad;
            Vector3 pos = pivot + new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw)) * orbitDistance;
            pos.y = pivot.y + orbitHeight;
            transform.position = pos;
            transform.LookAt(pivot);
            yield return null;
        }
        follow.enabled = true;   // CameraFollow lerps from here to its own pose
        Destroy(this);
    }
}
