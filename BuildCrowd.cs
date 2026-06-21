using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// One-off: build the finale "sea of people" — a field of ~220 dark low-poly
// figures in a void far from the lounge, a camera anchor to view them, plus a
// full-screen black ScreenFade overlay for the transition. Idempotent.
public static class BuildCrowd
{
    public static string Execute()
    {
        // clean previous build
        var old = GameObject.Find("CrowdScene");
        if (old != null) Object.DestroyImmediate(old);

        // ---- silhouette material (unlit, near-black, instanced) ----
        const string matDir = "Assets/Materials/Lounge";
        const string matPath = matDir + "/M_Silhouette.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            if (!AssetDatabase.IsValidFolder(matDir)) AssetDatabase.CreateFolder("Assets/Materials", "Lounge");
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.SetColor("_BaseColor", new Color(0.02f, 0.02f, 0.035f, 1f));
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);

        // ---- root far from the lounge (lounge spans ~z -18..18) ----
        var root = new GameObject("CrowdScene");
        root.transform.position = new Vector3(0f, 0f, 500f);

        // dark ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Object.DestroyImmediate(ground.GetComponent<Collider>());
        ground.name = "VoidGround";
        ground.transform.SetParent(root.transform, false);
        ground.transform.localPosition = new Vector3(0f, 0f, 40f);
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        ground.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // figure template
        var template = MakeFigure(mat);
        template.transform.SetParent(root.transform, false);

        var crowd = new GameObject("Crowd");
        crowd.transform.SetParent(root.transform, false);

        int rows = 14, perRow = 16;
        float zStep = 5.2f, xSpread = 78f;
        var rnd = new System.Random(12345);
        int made = 0;
        for (int r = 0; r < rows; r++)
        {
            float z = r * zStep;
            float widen = 1f + r * 0.05f;           // fan out slightly with distance
            for (int c = 0; c < perRow; c++)
            {
                float tx = perRow == 1 ? 0.5f : (float)c / (perRow - 1);
                float x = Mathf.Lerp(-xSpread * 0.5f, xSpread * 0.5f, tx) * widen;
                x += (float)(rnd.NextDouble() * 3 - 1.5);
                float zz = z + (float)(rnd.NextDouble() * 3 - 1.5);

                var fig = Object.Instantiate(template, crowd.transform);
                fig.transform.localPosition = new Vector3(x, 0f, zz);
                float s = 0.9f + (float)rnd.NextDouble() * 0.28f;
                fig.transform.localScale = new Vector3(s, s, s);
                fig.transform.localEulerAngles = new Vector3(0f, (float)(rnd.NextDouble() * 360), 0f);
                made++;
            }
        }
        Object.DestroyImmediate(template);   // template was just a source

        // camera anchor: in front of the crowd, looking into it
        var anchor = new GameObject("CrowdCamAnchor");
        anchor.transform.SetParent(root.transform, false);
        anchor.transform.localPosition = new Vector3(0f, 4.5f, -26f);
        anchor.transform.LookAt(root.transform.position + new Vector3(0f, 1.4f, 24f));

        // ---- ScreenFade overlay (full-screen black) ----
        var canvas = GameObject.Find("Canvas").transform;
        var oldFade = canvas.Find("ScreenFade");
        if (oldFade != null) Object.DestroyImmediate(oldFade.gameObject);
        var fadeGo = new GameObject("ScreenFade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        fadeGo.transform.SetParent(canvas, false);
        var frt = (RectTransform)fadeGo.transform;
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
        fadeGo.GetComponent<Image>().color = Color.black;
        var fcg = fadeGo.GetComponent<CanvasGroup>();
        fcg.alpha = 0f; fcg.blocksRaycasts = false; fcg.interactable = false;
        // keep it on top
        fadeGo.transform.SetAsLastSibling();

        // soften the closing dim so the crowd stays visible behind the text
        var closing = canvas.Find("ClosingMessagePanel");
        if (closing != null)
        {
            var cimg = closing.GetComponent<Image>();
            if (cimg != null) { var col = cimg.color; col.a = 0.18f; cimg.color = col; EditorUtility.SetDirty(cimg); }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Crowd built: " + made + " figures + ground + cam anchor + ScreenFade overlay.";
    }

    static GameObject MakeFigure(Material mat)
    {
        var f = new GameObject("Fig");
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.transform.SetParent(f.transform, false);
        body.transform.localScale = new Vector3(0.55f, 0.9f, 0.35f);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.GetComponent<MeshRenderer>().sharedMaterial = mat;

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.transform.SetParent(f.transform, false);
        head.transform.localScale = Vector3.one * 0.5f;
        head.transform.localPosition = new Vector3(0f, 1.95f, 0f);
        head.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return f;
    }
}
