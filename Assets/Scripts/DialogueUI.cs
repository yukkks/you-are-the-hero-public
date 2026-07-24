using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shared speech panel: shows a colleague's name + their lines one screen at a
// time, typed out character by character. Clicking Next mid-type completes the
// line instantly; the last screen's button reads "Close".
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    // True while a colleague is speaking — movement + tap-to-move freeze,
    // and the on-screen joystick hides so nothing overlaps the panel.
    public static bool IsOpen { get; private set; }

    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text bodyText;

    [Header("Next / Close button")]
    public Button nextButton;
    public TMP_Text nextLabel;

    [Header("Hide while talking (joystick etc.)")]
    public GameObject[] hideWhileOpen;

    [Header("Typewriter")]
    public float charsPerSecond = 45f;

    string[] lines;
    int index;
    Coroutine typing;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
    }

    public void Show(string who, string[] newLines) { Show(who, newLines, null); }

    // portrait arg kept for call-compatibility but ignored — no picture box.
    public void Show(string who, string[] newLines, Sprite portrait)
    {
        if (panel == null || newLines == null || newLines.Length == 0) return;
        lines = newLines;
        index = 0;
        if (nameText != null) nameText.text = who;
        IsOpen = true;
        if (hideWhileOpen != null)
            foreach (var g in hideWhileOpen) if (g != null) g.SetActive(false);
        panel.SetActive(true);
        ShowCurrent();
    }

    void ShowCurrent()
    {
        if (bodyText == null) return;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(TypeLine(lines[index]));
        bool last = index >= lines.Length - 1;
        if (nextLabel != null) nextLabel.text = last ? "Close" : "Next ›";
    }

    IEnumerator TypeLine(string line)
    {
        bodyText.text = line;
        bodyText.maxVisibleCharacters = 0;
        float shown = 0f;
        while (bodyText.maxVisibleCharacters < line.Length)
        {
            shown += charsPerSecond * Time.deltaTime;
            bodyText.maxVisibleCharacters = Mathf.Min(line.Length, Mathf.FloorToInt(shown));
            yield return null;
        }
        typing = null;
    }

    public void Next()
    {
        // first click on a still-typing line completes it instead of advancing
        if (typing != null && bodyText != null)
        {
            StopCoroutine(typing);
            typing = null;
            bodyText.maxVisibleCharacters = int.MaxValue;
            return;
        }
        index++;
        if (lines == null || index >= lines.Length) { Hide(); return; }
        ShowCurrent();
    }

    public void Hide()
    {
        if (typing != null) { StopCoroutine(typing); typing = null; }
        if (panel != null) panel.SetActive(false);
        IsOpen = false;
        if (hideWhileOpen != null)
            foreach (var g in hideWhileOpen) if (g != null) g.SetActive(true);
    }
}
