using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// Restyle the dialogue to a clean, light, rounded card (Messenger-by-abeto vibe,
// on-brand with the cream/amber/brown palette): soft shadow, brown text, amber
// name, rounded pill Next button.
public static class StyleDialogue
{
    public static string Execute()
    {
        var bg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        var canvas = GameObject.Find("Canvas").transform;

        Color cream     = new Color(0.961f, 0.906f, 0.827f, 0.98f);
        Color brown     = new Color(0.227f, 0.165f, 0.118f, 1f);
        Color amberDeep = new Color(0.914f, 0.561f, 0.180f, 1f);
        Color amber     = new Color(0.980f, 0.702f, 0.271f, 1f);

        // ---- Panel: cream rounded card + soft shadow ----
        var panel = canvas.Find("DialoguePanel").gameObject;
        var pimg = panel.GetComponent<Image>();
        pimg.sprite = bg; pimg.type = Image.Type.Sliced;
        pimg.pixelsPerUnitMultiplier = 0.18f;    // larger corner radius -> rounder card
        pimg.color = cream;
        var prt = (RectTransform)panel.transform;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(940f, 300f);
        prt.anchoredPosition = new Vector2(0f, 90f);
        var psh = panel.GetComponent<Shadow>() ?? panel.AddComponent<Shadow>();
        psh.effectColor = new Color(0.18f, 0.12f, 0.07f, 0.28f);
        psh.effectDistance = new Vector2(0f, -10f);
        EditorUtility.SetDirty(pimg); EditorUtility.SetDirty(psh);

        // ---- Name: amber, semibold, small ----
        var nameTmp = canvas.Find("DialoguePanel/Name").GetComponent<TextMeshProUGUI>();
        nameTmp.color = amberDeep;
        nameTmp.fontSize = 38f;
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.characterSpacing = 4f;
        nameTmp.alignment = TextAlignmentOptions.TopLeft;
        var nrt = nameTmp.rectTransform;
        nrt.anchorMin = new Vector2(0f, 1f); nrt.anchorMax = new Vector2(1f, 1f); nrt.pivot = new Vector2(0.5f, 1f);
        nrt.offsetMin = new Vector2(54f, -86f);
        nrt.offsetMax = new Vector2(-54f, -30f);
        EditorUtility.SetDirty(nameTmp);

        // ---- Body: warm brown, comfortable ----
        var bodyTmp = canvas.Find("DialoguePanel/Body").GetComponent<TextMeshProUGUI>();
        bodyTmp.color = brown;
        bodyTmp.fontSize = 37f;
        bodyTmp.alignment = TextAlignmentOptions.TopLeft;
        bodyTmp.textWrappingMode = TextWrappingModes.Normal;
        var brt = bodyTmp.rectTransform;
        brt.anchorMin = new Vector2(0f, 0f); brt.anchorMax = new Vector2(1f, 1f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.offsetMin = new Vector2(54f, 96f);    // leave room for the button strip
        brt.offsetMax = new Vector2(-54f, -98f);
        EditorUtility.SetDirty(bodyTmp);

        // ---- Next button: amber pill, brown label, centered bottom ----
        var btn = canvas.Find("DialoguePanel/NextButton").gameObject;
        var bimg = btn.GetComponent<Image>();
        bimg.sprite = bg; bimg.type = Image.Type.Sliced;
        bimg.pixelsPerUnitMultiplier = 0.12f;     // very round -> pill
        bimg.color = amber;
        var bbrt = (RectTransform)btn.transform;
        bbrt.anchorMin = bbrt.anchorMax = new Vector2(0.5f, 0f);
        bbrt.pivot = new Vector2(0.5f, 0f);
        bbrt.sizeDelta = new Vector2(186f, 68f);
        bbrt.anchoredPosition = new Vector2(0f, 24f);
        var bsh = btn.GetComponent<Shadow>() ?? btn.AddComponent<Shadow>();
        bsh.effectColor = new Color(0.79f, 0.47f, 0.12f, 0.55f);
        bsh.effectDistance = new Vector2(0f, -5f);
        var blbl = btn.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        blbl.color = brown;
        blbl.fontSize = 30f;
        blbl.fontStyle = FontStyles.Bold;
        EditorUtility.SetDirty(bimg); EditorUtility.SetDirty(bsh); EditorUtility.SetDirty(blbl);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Dialogue restyled: cream rounded card, amber name, pill Next button.";
    }
}
