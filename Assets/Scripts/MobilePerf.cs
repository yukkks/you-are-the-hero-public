using UnityEngine;

// Mobile framerate & quality guards, applied at startup. A steady 30 feels
// smoother than a jittery 45-60 on a phone, and these settings trim the most
// expensive per-frame costs without touching the flame's dynamic lighting.
public class MobilePerf : MonoBehaviour
{
    public int targetFps = 30;

    void Awake()
    {
        Application.targetFrameRate = targetFps;
        QualitySettings.vSyncCount = 0;                 // let targetFrameRate rule
        QualitySettings.shadows = ShadowQuality.Disable; // realtime shadows are the #1 mobile GPU cost
        QualitySettings.shadowDistance = 0f;
        QualitySettings.skinWeights = SkinWeights.TwoBones;   // cheaper skinning for the avatars
        QualitySettings.softParticles = false;
        QualitySettings.billboardsFaceCameraPosition = false;
        QualitySettings.lodBias = 1.4f;                 // switch to cheaper LODs a bit sooner
    }
}
