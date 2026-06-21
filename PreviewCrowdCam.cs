using UnityEditor;
using UnityEngine;

// Position the Scene View at the finale crowd camera anchor so we can preview the
// actual composition (capture_scene_object with no path afterwards).
public static class PreviewCrowdCam
{
    public static string Execute()
    {
        var a = GameObject.Find("CrowdCamAnchor").transform;
        var sv = SceneView.lastActiveSceneView;
        sv.pivot = a.position + a.forward * 26f;
        sv.rotation = a.rotation;
        sv.size = 22f;
        sv.Repaint();
        return "Scene view moved to crowd cam anchor.";
    }
}
