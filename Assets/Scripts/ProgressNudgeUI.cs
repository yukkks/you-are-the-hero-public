using UnityEngine;
using TMPro;

// Gentle wayfinding for a non-gamer: always shows how much of the farewell is left.
public class ProgressNudgeUI : MonoBehaviour
{
    public TMP_Text label;
    [TextArea] public string format = "Goodbyes shared  {0}/{1}\nMemories found  {2}/{3}";

    void Start()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnProgressChanged.AddListener(Refresh);
        Refresh();
    }

    public void Refresh()
    {
        var gp = GameProgress.Instance;
        if (label == null || gp == null) return;
        label.text = string.Format(format, gp.GreetedCount, gp.NpcTotal, gp.PhotosViewedCount, gp.PhotoTotal);
    }
}
