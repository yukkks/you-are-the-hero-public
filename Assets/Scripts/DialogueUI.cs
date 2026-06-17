using UnityEngine;
using TMPro;

// Shared speech panel: shows a colleague's name + line, then auto-hides.
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public float autoHideSeconds = 5f;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Show(string who, string line)
    {
        if (panel == null) return;
        if (nameText != null) nameText.text = who;
        if (bodyText != null) bodyText.text = line;
        panel.SetActive(true);
        CancelInvoke(nameof(Hide));
        if (autoHideSeconds > 0f) Invoke(nameof(Hide), autoHideSeconds);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
