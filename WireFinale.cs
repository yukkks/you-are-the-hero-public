using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// One-off: wire the FinaleController references, add a CanvasGroup for the message
// fade, and restyle ClosingMessagePanel into a full-screen cinematic dim.
public static class WireFinale
{
    public static string Execute()
    {
        var fc = GameObject.Find("GameManager").GetComponent<FinaleController>();
        var canvas = GameObject.Find("Canvas").transform;

        fc.hero = GameObject.Find("Player").transform;
        var camGo = GameObject.Find("Main Camera");
        fc.cam = camGo.GetComponent<Camera>();
        fc.cameraRig = camGo.GetComponent<CameraFollow>();
        fc.playerControl = GameObject.Find("Player").GetComponent<ClickToMove>();
        fc.joystick = canvas.Find("MoveJoystick").gameObject;
        fc.hideOnFinale = new GameObject[] {
            canvas.Find("ProgressNudge").gameObject,
            canvas.Find("DialoguePanel").gameObject,
        };

        fc.npcs = new Transform[] {
            GameObject.Find("Characters/YuAttia").transform,
            GameObject.Find("Characters/Rebecca").transform,
            GameObject.Find("Characters/Paul").transform,
            GameObject.Find("Characters/Shikha").transform,
            GameObject.Find("Characters/model-14").transform,
        };

        fc.closingMessage =
            "[PLACEHOLDER] Wherever you go, you carry a piece of all of us.\nThank you — for everything.";
        fc.crowdLine = "You touched more than 1,500 lives.";
        fc.screenFade = canvas.Find("ScreenFade").GetComponent<CanvasGroup>();
        fc.crowdCamAnchor = GameObject.Find("CrowdCamAnchor").transform;
        fc.crowdRoot = GameObject.Find("CrowdScene");
        fc.crowdRoot.SetActive(false);   // hidden until the finale

        // ---- Closing panel: full-screen dim + centered message ----
        var panel = canvas.Find("ClosingMessagePanel").gameObject;
        var pimg = panel.GetComponent<Image>();
        pimg.color = new Color(0.04f, 0.03f, 0.02f, 0.82f);
        pimg.sprite = null;                          // plain rectangle, full bleed
        var prt = (RectTransform)panel.transform;
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;                   // must not block gameplay clicks while invisible
        fc.messageGroup = cg;

        var msg = canvas.Find("ClosingMessagePanel/Message").GetComponent<TextMeshProUGUI>();
        msg.color = new Color(0.96f, 0.93f, 0.86f);
        msg.fontSize = 70f;
        msg.fontStyle = FontStyles.Bold;
        msg.alignment = TextAlignmentOptions.Center;
        msg.textWrappingMode = TextWrappingModes.Normal;
        var mrt = msg.rectTransform;
        mrt.anchorMin = new Vector2(0f, 0.32f); mrt.anchorMax = new Vector2(1f, 0.68f);
        mrt.offsetMin = new Vector2(90f, 0f); mrt.offsetMax = new Vector2(-90f, 0f);
        fc.messageText = msg;

        EditorUtility.SetDirty(fc);
        EditorUtility.SetDirty(pimg);
        EditorUtility.SetDirty(msg);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Finale wired: hero, 5 npcs, camera, controls, closing panel restyled + faded group.";
    }
}
