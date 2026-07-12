using UnityEngine;

// A small warm floating orb above anything the hero hasn't discovered yet.
// Bobs gently, pulses its glow, and fades away once its host is greeted/viewed.
public class InteractableBeacon : MonoBehaviour
{
    public float height = 0.35f;   // above the host's renderer bounds
    public Color glow = new Color(1f, 0.78f, 0.35f);

    Transform orb;
    Material mat;
    float baseY;
    float phase;
    bool fading;
    float alpha = 1f;

    void Start()
    {
        phase = Random.value * 10f;

        // top of the host's combined bounds
        var rends = GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(transform.position, Vector3.one * 0.2f);
        bool first = true;
        foreach (var r in rends)
        {
            if (r.GetComponent<ParticleSystem>() != null) continue;
            if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
        }
        baseY = b.max.y + height;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.name = "Beacon";
        go.transform.SetParent(transform, true);
        go.transform.position = new Vector3(b.center.x, baseY, b.center.z);
        go.transform.localScale = Vector3.one * 0.16f;
        orb = go.transform;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = glow;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", glow * 2.2f);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        if (GameProgress.Instance != null)
            GameProgress.Instance.OnProgressChanged.AddListener(CheckDone);
    }

    void CheckDone()
    {
        if (fading || GameProgress.Instance == null) return;
        bool done =
            (GetComponent<NPCDialogue>() != null && GameProgress.Instance.IsGreeted(gameObject)) ||
            (GetComponent<PhotoFrameInteract>() != null && GameProgress.Instance.IsPhotoViewed(gameObject));
        if (done) fading = true;
    }

    void Update()
    {
        if (orb == null) return;

        float t = Time.time + phase;
        Vector3 p = orb.position;
        p.y = baseY + Mathf.Sin(t * 1.6f) * 0.07f;
        orb.position = p;

        float pulse = 1.7f + 0.9f * (0.5f + 0.5f * Mathf.Sin(t * 2.4f));
        if (fading)
        {
            alpha -= Time.deltaTime * 1.5f;
            if (alpha <= 0f) { Destroy(orb.gameObject); Destroy(this); return; }
            orb.localScale = Vector3.one * 0.16f * alpha;
        }
        mat.SetColor("_EmissionColor", glow * pulse * Mathf.Max(alpha, 0.01f));
    }
}
