using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

// Remove the now-dead in-Unity finale (the ending is an external video in the web
// wrapper). Drops the 224-figure crowd, the screen-fade overlay, the real-time
// FinaleController, and its leftover assets.
public static class CleanupFinale
{
    public static string Execute()
    {
        var log = new System.Text.StringBuilder("Cleanup: ");

        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name == "CrowdScene") { Object.DestroyImmediate(go); log.Append("CrowdScene; "); }

        var canvas = GameObject.Find("Canvas").transform;
        var sf = canvas.Find("ScreenFade");
        if (sf != null) { Object.DestroyImmediate(sf.gameObject); log.Append("ScreenFade; "); }

        var gm = GameObject.Find("GameManager");
        var fc = gm.GetComponent<FinaleController>();
        if (fc != null) { Object.DestroyImmediate(fc); log.Append("FinaleController; "); }

        if (AssetDatabase.DeleteAsset("Assets/Materials/Lounge/M_Silhouette.mat")) log.Append("M_Silhouette; ");
        if (AssetDatabase.DeleteAsset("Assets/Video/FinaleRT.renderTexture")) log.Append("FinaleRT; ");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return log.ToString();
    }
}
