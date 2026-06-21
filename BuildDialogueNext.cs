using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// One-off: build the dialogue "Next/Close" button, wire it into DialogueUI, and
// load each colleague's two placeholder sentences.
public static class BuildDialogueNext
{
    public static string Execute()
    {
        var bg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        var canvas = GameObject.Find("Canvas").transform;
        var panel = canvas.Find("DialoguePanel");

        // --- Next button (create only if missing) ---
        Transform existing = panel.Find("NextButton");
        GameObject btnGo;
        if (existing != null) btnGo = existing.gameObject;
        else
        {
            btnGo = new GameObject("NextButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel, false);
        }
        var brt = (RectTransform)btnGo.transform;
        brt.anchorMin = new Vector2(1f, 0f); brt.anchorMax = new Vector2(1f, 0f); brt.pivot = new Vector2(1f, 0f);
        brt.sizeDelta = new Vector2(240f, 92f);
        brt.anchoredPosition = new Vector2(-34f, 30f);
        var bimg = btnGo.GetComponent<Image>();
        bimg.sprite = bg; bimg.type = Image.Type.Sliced;
        bimg.color = new Color(0.98f, 0.70f, 0.27f, 1f);   // amber
        var btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = bimg;

        // label (TMP)
        Transform lblT = btnGo.transform.Find("Label");
        TextMeshProUGUI lbl;
        if (lblT != null) lbl = lblT.GetComponent<TextMeshProUGUI>();
        else
        {
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            lblGo.transform.SetParent(btnGo.transform, false);
            lbl = lblGo.AddComponent<TextMeshProUGUI>();
        }
        var lrt = lbl.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        lbl.text = "Next ›";
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.fontSize = 40f;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = new Color(0.16f, 0.11f, 0.06f, 1f);   // dark brown on amber
        lbl.raycastTarget = false;

        // --- wire into DialogueUI ---
        var dui = canvas.Find("DialogueUI").GetComponent<DialogueUI>();
        dui.nextButton = btn;
        dui.nextLabel = lbl;
        EditorUtility.SetDirty(dui);

        // --- load two placeholder sentences per colleague ---
        Set("Characters/YuAttia",
            "[PLACEHOLDER] Working alongside you has been one of the highlights of my career.",
            "[PLACEHOLDER] Wishing you nothing but the best on the road ahead — go get 'em.");
        Set("Characters/Rebecca",
            "[PLACEHOLDER] I'll never forget the times we shared here.",
            "[PLACEHOLDER] You made every day brighter. Take care out there, okay?");
        Set("Characters/Paul",
            "[PLACEHOLDER] It won't be the same around here without you.",
            "[PLACEHOLDER] Thank you for everything — now go do something amazing.");
        Set("Characters/Shikha",
            "[PLACEHOLDER] You taught me more than you know.",
            "[PLACEHOLDER] I'm going to miss our talks. Promise you'll stay in touch.");
        Set("Characters/model-14",
            "[PLACEHOLDER] From all of us — thank you, truly.",
            "[PLACEHOLDER] You've left a mark on this place that won't fade. Farewell, friend.");

        EditorUtility.SetDirty(bimg); EditorUtility.SetDirty(lbl);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Next button built + wired; 5 colleagues given 2 sentences each.";
    }

    static void Set(string path, string a, string b)
    {
        var go = GameObject.Find(path);
        if (go == null) return;
        var npc = go.GetComponent<NPCDialogue>();
        if (npc == null) return;
        npc.lines = new[] { a, b };
        EditorUtility.SetDirty(npc);
    }
}
