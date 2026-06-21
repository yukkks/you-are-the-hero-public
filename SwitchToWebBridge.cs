using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;

// Switch the finale from in-Unity playback to a web signal: remove the in-Unity
// video rig, add GameCompleteSignal, and repoint the Skip button to fire it.
public static class SwitchToWebBridge
{
    public static string Execute()
    {
        var canvas = GameObject.Find("Canvas").transform;

        // remove the (wrong) in-Unity video objects
        var rig = GameObject.Find("FinaleVideoRig");
        if (rig != null) Object.DestroyImmediate(rig);
        var screen = canvas.Find("FinaleScreen");
        if (screen != null) Object.DestroyImmediate(screen.gameObject);

        // add the completion signal to GameManager
        var gm = GameObject.Find("GameManager");
        var sig = gm.GetComponent<GameCompleteSignal>();
        if (sig == null) sig = gm.AddComponent<GameCompleteSignal>();

        // repoint the Skip button -> GameCompleteSignal.Fire
        var btn = canvas.Find("SkipFinaleButton").GetComponent<Button>();
        for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(btn.onClick, i);
        UnityEventTools.AddPersistentListener(btn.onClick, sig.Fire);

        // relabel the skip button
        var lbl = canvas.Find("SkipFinaleButton/Label").GetComponent<TMPro.TextMeshProUGUI>();
        if (lbl != null) lbl.text = "Skip ▶ (signal end)";

        EditorUtility.SetDirty(sig);
        EditorUtility.SetDirty(btn);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Switched to web bridge: in-Unity video removed, GameCompleteSignal added, Skip fires it.";
    }
}
