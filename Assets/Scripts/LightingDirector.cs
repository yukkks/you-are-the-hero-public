using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// The Last Flame's master dial. Progress 0 -> 1 lerps the WHOLE room's mood via
// global lighting parameters only (ambient, sun, fog) — no per-object lights, so
// it costs nothing and behaves identically in WebGL.
//   0.0  = the sleeping lounge: dim, cool, blue-grey (but always readable)
//   1.0  = fully awake: the warm golden-hour look
// `lit` is driven externally (flames given) and eased toward here.
public class LightingDirector : MonoBehaviour
{
    public static LightingDirector Instance { get; private set; }

    [Header("Dark (progress 0) — cave dark: only the flame lights the room")]
    public Color darkSky = new Color(0.015f, 0.018f, 0.030f);
    public Color darkEquator = new Color(0.008f, 0.010f, 0.018f);
    public Color darkGround = new Color(0.002f, 0.003f, 0.006f);
    public float darkSun = 0f;
    public Color darkSunColor = new Color(0.30f, 0.36f, 0.55f);
    public Color darkFog = new Color(0.006f, 0.008f, 0.016f);
    public float darkFogDensity = 0.09f;   // heavier haze = the dark swallows distance

    [Header("Warm (progress 1)")]
    public Color warmSky = new Color(1f, 0.91f, 0.80f);
    public Color warmEquator = new Color(0.85f, 0.75f, 0.64f);
    public Color warmGround = new Color(0.42f, 0.35f, 0.30f);
    public float warmSun = 1.15f;
    public Color warmSunColor = new Color(1f, 0.87f, 0.72f);
    public Color warmFog = new Color(0.86f, 0.80f, 0.70f);
    public float warmFogDensity = 0.006f;

    [Header("Response")]
    public float easeSpeed = 1.4f;   // how fast the room follows the target

    [Header("Dark veil (uniform screen darkening — the real cave-dark lever)")]
    [Range(0f, 1f)] public float maxVeil = 0.82f;   // opacity of the black veil at progress 0

    public float Target { get; private set; }   // 0..1, set by the flame system
    float current;
    Light sun;
    Camera cam;
    Color camClear;
    Material skybox;
    float skyExposure = 1f;
    Image veil;

    void Awake()
    {
        Instance = this;
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional && (sun == null || l.intensity > sun.intensity)) sun = l;
        cam = Camera.main;
        if (cam != null) camClear = cam.backgroundColor;
        skybox = RenderSettings.skybox;
        if (skybox != null && skybox.HasProperty("_Exposure")) skyExposure = skybox.GetFloat("_Exposure");
    }

    void Start()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        BuildVeil();
        current = Target;   // start wherever the target is (0 for the real game)
        Apply(current);
    }

    // A full-screen black Image on its own top-most canvas. Uniformly darkens the
    // whole frame regardless of material — the robust cave-dark lever, cheap and
    // WebGL-safe. Sits BELOW the game HUD canvas so buttons stay usable.
    void BuildVeil()
    {
        var go = new GameObject("DarkVeil", typeof(Canvas), typeof(CanvasScaler));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;   // above the world, below the HUD (which is higher)
        var imgGo = new GameObject("Veil", typeof(Image));
        imgGo.transform.SetParent(go.transform, false);
        veil = imgGo.GetComponent<Image>();
        veil.color = new Color(0f, 0f, 0f, 0f);
        veil.raycastTarget = false;   // never eats touches
        var rt = veil.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // 0..1 — how awake the room should be. Call as flames are given out.
    public void SetTarget(float t) { Target = Mathf.Clamp01(t); }

    void Update()
    {
        if (!Mathf.Approximately(current, Target))
        {
            current = Mathf.MoveTowards(current, Target, easeSpeed * Time.deltaTime);
            Apply(current);
        }
    }

    void Apply(float t)
    {
        float k = Mathf.SmoothStep(0f, 1f, t);
        RenderSettings.ambientSkyColor = Color.Lerp(darkSky, warmSky, k);
        RenderSettings.ambientEquatorColor = Color.Lerp(darkEquator, warmEquator, k);
        RenderSettings.ambientGroundColor = Color.Lerp(darkGround, warmGround, k);
        RenderSettings.fogColor = Color.Lerp(darkFog, warmFog, k);
        RenderSettings.fogDensity = Mathf.Lerp(darkFogDensity, warmFogDensity, k);
        if (sun != null)
        {
            sun.intensity = Mathf.Lerp(darkSun, warmSun, k);
            sun.color = Color.Lerp(darkSunColor, warmSunColor, k);
        }
        // Fade the sky itself out in the dark so the windows/backdrop go black
        // (cave dark) and bloom back to golden hour as she lights the room.
        if (skybox != null && skybox.HasProperty("_Exposure"))
            skybox.SetFloat("_Exposure", Mathf.Lerp(skyExposure * 0.02f, skyExposure, k));

        // Robust cave-dark: below a threshold, stop drawing the skybox entirely
        // (kills the sun/moon disc + lit backdrop) and clear to near-black. Blend
        // back to the skybox as the room wakes.
        if (cam != null)
        {
            if (k < 0.15f)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.Lerp(new Color(0.004f, 0.006f, 0.012f), camClear, k / 0.15f);
            }
            else cam.clearFlags = CameraClearFlags.Skybox;
        }

        // The veil: heaviest in the dark, gone by the time the room is warm.
        // Curved so it clears quickly in the last third (room feels "fully awake").
        if (veil != null)
        {
            float a = maxVeil * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(k / 0.85f)));
            veil.color = new Color(0f, 0f, 0f, a);
        }
    }

    // Editor/prototype helper: jump instantly to a value for capture.
    public void PreviewInstant(float t) { Target = Mathf.Clamp01(t); current = Target; Apply(current); }
}
