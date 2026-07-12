using UnityEngine;

// The gentlest breeze: a slow sub-degree sway so the trees don't feel frozen.
public class TreeSway : MonoBehaviour
{
    public float degrees = 0.7f;
    public float speed = 0.5f;

    Quaternion rest;
    float phase;

    void Start()
    {
        rest = transform.localRotation;
        phase = Random.value * 10f;
    }

    void Update()
    {
        float t = Time.time * speed + phase;
        transform.localRotation = rest * Quaternion.Euler(
            Mathf.Sin(t) * degrees,
            0f,
            Mathf.Sin(t * 0.83f + 1.3f) * degrees * 0.7f);
    }
}
