using UnityEngine;
using UnityEngine.UI;

// Shared full-screen photo popup. Owns the existing PhotoPopup panel + RawImage.
public class PhotoViewerUI : MonoBehaviour
{
    public static PhotoViewerUI Instance { get; private set; }

    public GameObject popup;
    public RawImage image;

    void Awake()
    {
        Instance = this;
        if (popup != null) popup.SetActive(false);
    }

    public void Show(Texture tex)
    {
        if (popup == null) return;
        if (tex != null && image != null) image.texture = tex;
        popup.SetActive(true);
    }

    // Wired to the close button.
    public void Close()
    {
        if (popup != null) popup.SetActive(false);
    }
}
