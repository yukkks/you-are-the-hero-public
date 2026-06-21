using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using TMPro;

// One-off: add a small on-screen "Skip to Finale" debug button (top-right) wired
// to FinaleController.TriggerFinale. Idempotent.
public static class BuildSkipButton
{
    public static string Execute()
    {
        var bg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        var canvas = GameObject.Find("Canvas").transform;
        var fc = GameObject.Find("GameManager").GetComponent<FinaleController>();

        var old = canvas.Find("SkipFinaleButton");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var go = new GameObject("SkipFinaleButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(canvas, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(250f, 78f);
        rt.anchoredPosition = new Vector2(-28f, -28f);
        var img = go.GetComponent<Image>();
        img.sprite = bg; img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 0.18f;
        img.color = new Color(0.13f, 0.10f, 0.07f, 0.78f);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        var lblGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        lblGo.transform.SetParent(go.transform, false);
        var lbl = lblGo.AddComponent<TextMeshProUGUI>();
        var lrt = lbl.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        lbl.text = "Skip to Finale ▶";
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.fontSize = 28f;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = new Color(0.96f, 0.93f, 0.86f);
        lbl.raycastTarget = false;

        // wire onClick -> FinaleController.TriggerFinale (persists in the scene)
        UnityEventTools.AddPersistentListener(btn.onClick, fc.TriggerFinale);

        // keep the black screen-fade on top
        var fade = canvas.Find("ScreenFade");
        if (fade != null) fade.SetAsLastSibling();

        EditorUtility.SetDirty(btn);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Skip-to-Finale button added (top-right) and wired.";
    }
}
