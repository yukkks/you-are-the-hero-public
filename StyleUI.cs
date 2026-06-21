using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// One-off editor pass: fix the CanvasScaler, give the joystick a standard round
// look, and restyle the dialogue box to a clean game-style panel.
// Uses Transform.Find (works on INACTIVE objects, unlike GameObject.Find).
public static class StyleUI
{
    public static string Execute()
    {
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var bg   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        var canvasGo = GameObject.Find("Canvas");
        var canvas = canvasGo.transform;

        // ---- CanvasScaler: scale with screen (portrait reference) ----
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(scaler);

        // ---- Joystick: round sprite, standard translucent look ----
        var baseGo = canvas.Find("MoveJoystick").gameObject;
        var knobGo = canvas.Find("MoveJoystick/Knob").gameObject;
        var baseImg = baseGo.GetComponent<Image>();
        var knobImg = knobGo.GetComponent<Image>();
        baseImg.sprite = knob; baseImg.type = Image.Type.Simple;
        baseImg.color = new Color(0.06f, 0.06f, 0.08f, 0.32f);
        knobImg.sprite = knob; knobImg.type = Image.Type.Simple;
        knobImg.color = new Color(1f, 1f, 1f, 0.92f);

        var baseRt = baseGo.GetComponent<RectTransform>();
        baseRt.anchorMin = baseRt.anchorMax = new Vector2(0f, 0f);
        baseRt.pivot = new Vector2(0.5f, 0.5f);
        baseRt.sizeDelta = new Vector2(340f, 340f);
        baseRt.anchoredPosition = new Vector2(250f, 280f);
        var knobRt = knobGo.GetComponent<RectTransform>();
        knobRt.anchorMin = knobRt.anchorMax = new Vector2(0.5f, 0.5f);
        knobRt.pivot = new Vector2(0.5f, 0.5f);
        knobRt.sizeDelta = new Vector2(150f, 150f);
        knobRt.anchoredPosition = Vector2.zero;
        baseGo.GetComponent<VirtualJoystick>().handleRange = 105f;
        EditorUtility.SetDirty(baseImg); EditorUtility.SetDirty(knobImg);

        // ---- Dialogue panel: clean bottom box (may be inactive) ----
        var panel = canvas.Find("DialoguePanel").gameObject;
        var panelImg = panel.GetComponent<Image>();
        panelImg.sprite = bg; panelImg.type = Image.Type.Sliced;
        panelImg.color = new Color(0.13f, 0.10f, 0.07f, 0.94f);
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 0f);
        pRt.pivot = new Vector2(0.5f, 0f);
        pRt.sizeDelta = new Vector2(980f, 320f);
        pRt.anchoredPosition = new Vector2(0f, 70f);
        EditorUtility.SetDirty(panelImg);

        // Name (amber, bold, top)
        var nameTmp = canvas.Find("DialoguePanel/Name").GetComponent<TextMeshProUGUI>();
        nameTmp.color = new Color(0.98f, 0.70f, 0.27f);
        nameTmp.fontSize = 46f;
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.alignment = TextAlignmentOptions.TopLeft;
        var nRt = nameTmp.rectTransform;
        nRt.anchorMin = new Vector2(0f, 1f); nRt.anchorMax = new Vector2(1f, 1f); nRt.pivot = new Vector2(0.5f, 1f);
        nRt.offsetMin = new Vector2(46f, -96f);
        nRt.offsetMax = new Vector2(-46f, -26f);
        EditorUtility.SetDirty(nameTmp);

        // Body (cream, wraps)
        var bodyTmp = canvas.Find("DialoguePanel/Body").GetComponent<TextMeshProUGUI>();
        bodyTmp.color = new Color(0.96f, 0.93f, 0.86f);
        bodyTmp.fontSize = 36f;
        bodyTmp.alignment = TextAlignmentOptions.TopLeft;
        bodyTmp.textWrappingMode = TextWrappingModes.Normal;
        var bRt = bodyTmp.rectTransform;
        bRt.anchorMin = new Vector2(0f, 0f); bRt.anchorMax = new Vector2(1f, 1f); bRt.pivot = new Vector2(0.5f, 0.5f);
        bRt.offsetMin = new Vector2(46f, 40f);
        bRt.offsetMax = new Vector2(-46f, -110f);
        EditorUtility.SetDirty(bodyTmp);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "UI styled: CanvasScaler + joystick (round) + dialogue panel.";
    }
}
