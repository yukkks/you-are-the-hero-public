using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A single gentle nudge for a non-gamer: soft text that fades in over the dark
// at the start ("Tap someone to say hello") and disappears the moment she taps
// her first colleague. One hint, then trust her.
public class FirstTouchHint : MonoBehaviour
{
    public static FirstTouchHint Instance { get; private set; }

    [TextArea] public string hintText = "Tap someone to say hello";
    public float appearAfter = 2.5f;   // let the intro settle first
    public float fadeTime = 1.4f;

    CanvasGroup group;
    TMP_Text label;
    bool dismissed;

    void Awake()
    {
        Instance = this;
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f; group.blocksRaycasts = false;

        var txtGo = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(transform, false);
        label = txtGo.GetComponent<TextMeshProUGUI>();
        label.text = hintText;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true; label.fontSizeMin = 22; label.fontSizeMax = 40;
        label.fontStyle = FontStyles.Italic;
        label.color = new Color(1f, 0.93f, 0.78f, 1f);
        var rt = label.rectTransform;
        rt.anchorMin = new Vector2(0.1f, 0.16f); rt.anchorMax = new Vector2(0.9f, 0.28f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void Start() { StartCoroutine(Appear()); }

    IEnumerator Appear()
    {
        yield return new WaitForSeconds(appearAfter);
        if (dismissed) yield break;
        yield return Fade(0f, 1f);
        // soft breathing pulse until she taps someone
        while (!dismissed)
        {
            group.alpha = 0.72f + 0.28f * Mathf.Sin(Time.time * 1.6f);
            yield return null;
        }
    }

    // called by NPCDialogue the first time she greets anyone
    public void Dismiss()
    {
        if (dismissed) return;
        dismissed = true;
        StartCoroutine(Fade(group.alpha, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime) { t += Time.deltaTime; group.alpha = Mathf.Lerp(from, to, t / fadeTime); yield return null; }
        group.alpha = to;
        if (to == 0f) label.gameObject.SetActive(false);
    }
}
