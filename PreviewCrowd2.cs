using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Activate the (normally-hidden) CrowdScene and aim the Scene View straight at it
// so we can see whether the figures render and how they read.
public static class PreviewCrowd2
{
    public static string Execute()
    {
        GameObject root = null;
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name == "CrowdScene") root = go;
        if (root == null) return "CrowdScene not found";
        root.SetActive(true);

        int figs = 0;
        var crowd = root.transform.Find("Crowd");
        if (crowd != null) figs = crowd.childCount;

        var sv = SceneView.lastActiveSceneView;
        sv.pivot = new Vector3(0f, 1.5f, 530f);   // crowd center
        sv.rotation = Quaternion.Euler(8f, 0f, 0f);
        sv.size = 45f;
        sv.Repaint();
        return "CrowdScene active; figures=" + figs + "; scene view aimed at crowd.";
    }
}
