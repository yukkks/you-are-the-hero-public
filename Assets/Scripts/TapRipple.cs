using UnityEngine;

// A soft golden ring that blooms where the player taps to walk — the game's
// way of saying "heard you". Spawns, expands, fades, removes itself.
public class TapRipple : MonoBehaviour
{
    static Material sharedMat;

    float life;
    const float Duration = 0.55f;
    Material mat;

    public static void Spawn(Vector3 worldPos)
    {
        var go = new GameObject("TapRipple", typeof(MeshFilter), typeof(MeshRenderer), typeof(TapRipple));
        go.transform.position = worldPos + Vector3.up * 0.03f;
        go.transform.localScale = new Vector3(0.5f, 0.012f, 0.5f);
        go.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("New-Cylinder.fbx");

        if (sharedMat == null)
        {
            sharedMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            sharedMat.SetFloat("_Surface", 1f);   // transparent
            sharedMat.SetFloat("_ZWrite", 0f);
            sharedMat.renderQueue = 3000;
            sharedMat.SetOverrideTag("RenderType", "Transparent");
            sharedMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        var r = go.GetComponent<MeshRenderer>();
        r.material = new Material(sharedMat);     // instance so each ripple fades alone
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        go.GetComponent<TapRipple>().mat = r.material;
    }

    void Update()
    {
        life += Time.deltaTime;
        float k = life / Duration;
        if (k >= 1f) { Destroy(gameObject); return; }
        float s = Mathf.Lerp(0.5f, 1.9f, Mathf.SmoothStep(0f, 1f, k));
        transform.localScale = new Vector3(s, 0.012f, s);
        if (mat != null) mat.color = new Color(1f, 0.82f, 0.42f, 0.55f * (1f - k));
    }
}
