using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Make the crowd actually readable: pure-black figures against a warm dusk
// background (silhouettes need a lighter backdrop), and a closer camera framing.
public static class FixCrowdLook
{
    public static string Execute()
    {
        // pure-black silhouette material
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Lounge/M_Silhouette.mat");
        if (mat != null) { mat.SetColor("_BaseColor", Color.black); EditorUtility.SetDirty(mat); }

        // warm dusk void color (used for camera background + fog)
        var fc = GameObject.Find("GameManager").GetComponent<FinaleController>();
        fc.voidColor = new Color(0.52f, 0.35f, 0.27f, 1f);
        EditorUtility.SetDirty(fc);

        // find CrowdScene even if inactive
        GameObject root = null;
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name == "CrowdScene") root = go;
        if (root != null)
        {
            root.SetActive(true);
            var anchor = root.transform.Find("CrowdCamAnchor");
            if (anchor != null)
            {
                anchor.localPosition = new Vector3(0f, 3f, -16f);
                anchor.LookAt(root.transform.position + new Vector3(0f, 1.2f, 30f));
            }
            root.SetActive(false);   // hidden until the finale turns it on
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene());
        return "Crowd look fixed: black figures, warm dusk void, closer camera.";
    }
}
