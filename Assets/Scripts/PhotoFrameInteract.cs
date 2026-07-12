using UnityEngine;

// A framed memory on the wall. Click it -> the hero walks over -> the photo is
// unveiled full-screen and progress is marked.
public class PhotoFrameInteract : MonoBehaviour, IInteractable
{
    [Header("Photo")]
    public Texture photoTexture;
    [TextArea(1, 2)] public string caption = "";

    [Header("Where the hero stands to view (optional)")]
    public Transform viewAnchor;   // falls back to this object's position

    public Vector3 InteractPosition => viewAnchor != null ? viewAnchor.position : transform.position;

    void Start()
    {
        if (GameProgress.Instance != null) GameProgress.Instance.RegisterPhoto(gameObject);
    }

    public void Interact()
    {
        if (PhotoViewerUI.Instance != null) PhotoViewerUI.Instance.Show(photoTexture, caption);
        if (GameProgress.Instance != null && GameProgress.Instance.MarkPhotoViewed(gameObject))
            GameAudio.PlayPhoto();
    }
}
