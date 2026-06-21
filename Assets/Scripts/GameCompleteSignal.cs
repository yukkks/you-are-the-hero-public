using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// When everything is complete, tell the hosting web page (the landing/wrapper)
// so it can play the ending video OUTSIDE Unity. The ending is not rendered in
// Unity — Unity just fires this signal (the dog-runner postMessage pattern).
public class GameCompleteSignal : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void NotifyGameComplete();
#endif

    bool fired;

    void Start()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnAllComplete.AddListener(Fire);
    }

    public void Fire()
    {
        if (fired) return;
        fired = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        NotifyGameComplete();
#else
        Debug.Log("[GameCompleteSignal] complete -> postMessage {type:'gameComplete'} to the web wrapper (no-op in editor).");
#endif
    }
}
