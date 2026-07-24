using UnityEngine;

// The player's flame: one warm point light riding at eye level, with a soft
// organic flicker. Its brightness = flames she still holds, so it visibly dims
// as she gives them away (the story's cost mechanic). Shadowless — the only
// per-pixel cost is one cheap light, safe on old phones.
public class CarriedFlame : MonoBehaviour
{
    public Transform followTarget;    // the camera (set at spawn)
    public float baseIntensity = 3.4f;
    public float range = 11f;
    public Color color = new Color(1f, 0.66f, 0.30f);
    public Vector3 offset = new Vector3(0.28f, -0.30f, 0.62f);   // held in view, lower-right

    [Range(0f, 1f)] public float fuel = 1f;   // 1 = full flame, 0 = spent
    Light flame;
    Transform handleTf, flameTf;
    Material flameMat;
    ParticleSystem emberPs;
    float flickerSeed;

    void Awake()
    {
        var go = new GameObject("FlameLight");
        go.transform.SetParent(transform, false);
        flame = go.AddComponent<Light>();
        flame.type = LightType.Point;
        flame.color = color;
        flame.range = range;
        flame.shadows = LightShadows.None;
        flame.renderMode = LightRenderMode.ForcePixel;   // it's the hero light
        flickerSeed = 0f;

        BuildTorch();
    }

    // A simple candle/torch held in view: a dark wooden handle + a glowing
    // teardrop flame at the tip. Meshes come from PrimitiveLibrary (build-safe).
    void BuildTorch()
    {
        if (PrimitiveLibrary.Cylinder == null || PrimitiveLibrary.Sphere == null) return;

        var handle = new GameObject("TorchHandle", typeof(MeshFilter), typeof(MeshRenderer));
        handle.transform.SetParent(transform, false);
        handle.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        handle.transform.localScale = new Vector3(0.03f, 0.16f, 0.03f);
        handle.GetComponent<MeshFilter>().sharedMesh = PrimitiveLibrary.Cylinder;
        var hm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hm.color = new Color(0.20f, 0.13f, 0.08f);
        handle.GetComponent<MeshRenderer>().sharedMaterial = hm;
        handle.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        handleTf = handle.transform;

        var fl = new GameObject("FlameTip", typeof(MeshFilter), typeof(MeshRenderer));
        fl.transform.SetParent(transform, false);
        fl.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        fl.transform.localScale = new Vector3(0.055f, 0.11f, 0.055f);   // teardrop
        fl.GetComponent<MeshFilter>().sharedMesh = PrimitiveLibrary.Sphere;
        flameMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        flameMat.color = new Color(1f, 0.55f, 0.15f);
        flameMat.EnableKeyword("_EMISSION");
        flameMat.SetColor("_EmissionColor", color * 3f);
        var fr = fl.GetComponent<MeshRenderer>();
        fr.sharedMaterial = flameMat;
        fr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        flameTf = fl.transform;

        BuildEmbers(fl.transform);
    }

    // Rising embers/sparks from the flame tip — cheap particles, big warmth.
    void BuildEmbers(Transform tip)
    {
        var go = new GameObject("Embers");
        go.transform.SetParent(tip, false);
        go.transform.localPosition = Vector3.zero;
        emberPs = go.AddComponent<ParticleSystem>();
        var main = emberPs.main;
        main.startLifetime = 0.9f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.7f, 0.3f), new Color(1f, 0.4f, 0.12f));
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.08f;   // drift upward
        var emission = go.GetComponent<ParticleSystem>().emission; emission.rateOverTime = 22f;
        var shape = emberPs.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f;
        var vel = emberPs.velocityOverLifetime;
        vel.enabled = true; vel.y = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        var col = emberPs.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.75f, 0.35f), 0f), new GradientColorKey(new Color(0.9f, 0.3f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
        var psr = go.GetComponent<ParticleSystemRenderer>();
        if (PrimitiveLibrary.Particle != null) psr.material = PrimitiveLibrary.Particle;
        psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.TransformPoint(offset);
        if (followTarget != null)
            transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, 0.5f);

        float t = Time.time;
        float flick = 1f
            + 0.08f * Mathf.Sin(t * 11f + flickerSeed)
            + 0.05f * Mathf.Sin(t * 19f + 2.1f)
            + 0.04f * Mathf.Sin(t * 31f + 4.7f);
        float f = Mathf.Clamp01(fuel);

        flame.intensity = baseIntensity * f * flick;
        flame.range = range * Mathf.Lerp(0.4f, 1f, f);

        if (flameTf != null)
        {
            // the visible flame shrinks and dances as fuel drops
            float s = Mathf.Lerp(0.35f, 1f, f);
            flameTf.localScale = new Vector3(0.055f, 0.11f, 0.055f) * s * flick;
            flameMat.SetColor("_EmissionColor", color * 3f * f * flick);
        }
    }

    // Called when she gives a flame away; smoothly reduces what she carries.
    public void SetFuel(float f) { fuel = Mathf.Clamp01(f); }
}
