using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A full-screen title-style text overlay for story beats (the finale lines).
// One line at a time: fades up over black-ish, holds, fades out.
public class StoryOverlay : MonoBehaviour
{
    public static StoryOverlay Instance { get; private set; }

    CanvasGroup group;
    TMP_Text text;
    Image scrim;

    void Awake()
    {
        Instance = this;
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;   // above the veil, below nothing important
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f; group.blocksRaycasts = false;

        var scrimGo = new GameObject("Scrim", typeof(RectTransform), typeof(Image));
        scrimGo.transform.SetParent(transform, false);
        scrim = scrimGo.GetComponent<Image>();
        scrim.color = new Color(0f, 0f, 0f, 0.55f);
        Stretch(scrim.rectTransform);

        var txtGo = new GameObject("Line", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(transform, false);
        text = txtGo.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true; text.fontSizeMin = 24; text.fontSizeMax = 46;
        text.fontStyle = FontStyles.Italic;
        text.color = new Color(1f, 0.93f, 0.78f);
        var rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.1f, 0.35f); rt.anchorMax = new Vector2(0.9f, 0.65f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    public IEnumerator Play(string line, float hold)
    {
        text.text = line;
        yield return Fade(0f, 1f, 1.1f);
        yield return new WaitForSeconds(hold);
        yield return Fade(1f, 0f, 1.1f);
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / dur)); yield return null; }
        group.alpha = to;
    }
}
