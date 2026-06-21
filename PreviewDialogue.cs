using UnityEngine;
using TMPro;

// Temporarily show the dialogue panel with sample text so it can be captured.
// Call Hide() afterwards to restore.
public static class PreviewDialogue
{
    public static string Execute()
    {
        var canvas = GameObject.Find("Canvas").transform;
        var panel = canvas.Find("DialoguePanel").gameObject;
        canvas.Find("DialoguePanel/Name").GetComponent<TextMeshProUGUI>().text = "REBECCA";
        canvas.Find("DialoguePanel/Body").GetComponent<TextMeshProUGUI>().text =
            "I'll never forget the times we shared here. You made every day brighter.";
        canvas.Find("DialoguePanel/NextButton/Label").GetComponent<TextMeshProUGUI>().text = "Next ›";
        panel.SetActive(true);
        return "Dialogue panel shown for preview.";
    }

    public static string Hide()
    {
        GameObject.Find("Canvas").transform.Find("DialoguePanel").gameObject.SetActive(false);
        return "Dialogue panel hidden.";
    }
}
