using System.Collections;
using UnityEngine;
using TMPro;

// The in-lounge gathering finale. When every colleague has been greeted and every
// memory unveiled, the colleagues gather around the hero and a closing message
// appears. Skeleton: gathers by easing NPCs to waypoints. Polish later with Timeline
// + real animations. Subscribes itself to GameProgress.OnAllComplete.
public class FinaleController : MonoBehaviour
{
    [Header("Colleagues to gather")]
    public Transform[] npcs;

    [Header("Gather spots (the NPC_Point markers)")]
    public Transform[] gatherPoints;

    [Header("Timing")]
    public float gatherDuration = 2.5f;

    [Header("Closing message")]
    public GameObject messagePanel;
    public TMP_Text messageText;
    [TextArea] public string closingMessage = "Thank you for everything.";

    private bool started;

    void Start()
    {
        if (messagePanel != null) messagePanel.SetActive(false);
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnAllComplete.AddListener(TriggerFinale);
    }

    public void TriggerFinale()
    {
        if (started) return;
        started = true;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        int n = Mathf.Min(npcs.Length, gatherPoints.Length);
        var starts = new Vector3[n];
        for (int i = 0; i < n; i++)
            if (npcs[i] != null) starts[i] = npcs[i].position;

        float t = 0f;
        while (t < gatherDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / gatherDuration);
            for (int i = 0; i < n; i++)
                if (npcs[i] != null && gatherPoints[i] != null)
                    npcs[i].position = Vector3.Lerp(starts[i], gatherPoints[i].position, k);
            yield return null;
        }

        if (messagePanel != null) messagePanel.SetActive(true);
        if (messageText != null) messageText.text = closingMessage;
    }
}
