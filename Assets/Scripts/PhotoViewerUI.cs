using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shared full-screen photo popup with a slow Ken Burns drift and an optional
// caption line under the photo.
public class PhotoViewerUI : MonoBehaviour
{
    public static PhotoViewerUI Instance { get; private set; }

    public GameObject popup;
    public RawImage image;
    public TMP_Text captionText;   // optional

    Coroutine drift;

    void Awake()
    {
        Instance = this;
        if (popup != null) popup.SetActive(false);
    }

    public void Show(Texture tex) { Show(tex, null); }

    public void Show(Texture tex, string caption)
    {
        if (popup == null) return;
        if (tex != null && image != null) image.texture = tex;
        if (captionText != null)
        {
            captionText.text = string.IsNullOrEmpty(caption) ? "" : caption;
            captionText.gameObject.SetActive(!string.IsNullOrEmpty(caption));
        }
        popup.SetActive(true);
        if (image != null)
        {
            if (drift != null) StopCoroutine(drift);
            drift = StartCoroutine(KenBurns());
        }
    }

    IEnumerator KenBurns()
    {
        var rt = image.rectTransform;
        Vector3 baseScale = Vector3.one;
        rt.localScale = baseScale;
        float t = 0f;
        while (popup != null && popup.activeSelf)
        {
            t += Time.deltaTime;
            float s = 1f + 0.06f * Mathf.SmoothStep(0f, 1f, Mathf.Min(t / 14f, 1f));
            rt.localScale = baseScale * s;
            rt.anchoredPosition = new Vector2(Mathf.Sin(t * 0.11f) * 6f, Mathf.Cos(t * 0.09f) * 4f);
            yield return null;
        }
        rt.localScale = baseScale;
        rt.anchoredPosition = Vector2.zero;
        drift = null;
    }

    // Wired to the close button.
    public void Close()
    {
        if (popup != null) popup.SetActive(false);
    }
}
