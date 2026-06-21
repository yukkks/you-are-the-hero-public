using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shared speech panel: shows a colleague's name + their lines one screen at a
// time. The player clicks Next to advance; the last screen reads "Close".
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text bodyText;

    [Header("Next / Close button")]
    public Button nextButton;
    public TMP_Text nextLabel;

    string[] lines;
    int index;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
    }

    public void Show(string who, string[] newLines)
    {
        if (panel == null || newLines == null || newLines.Length == 0) return;
        lines = newLines;
        index = 0;
        if (nameText != null) nameText.text = who;
        panel.SetActive(true);
        ShowCurrent();
    }

    void ShowCurrent()
    {
        if (bodyText != null) bodyText.text = lines[index];
        bool last = index >= lines.Length - 1;
        if (nextLabel != null) nextLabel.text = last ? "Close" : "Next ›";
    }

    public void Next()
    {
        index++;
        if (lines == null || index >= lines.Length) { Hide(); return; }
        ShowCurrent();
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
