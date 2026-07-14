using System.Collections;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Tells the hosting web page the lounge is actually playable (the iframe
// 'load' event fires long before Unity finishes booting).
public class GameReadySignal : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void NotifyGameReady();
#endif

    IEnumerator Start()
    {
        yield return null;                       // one full frame rendered
        yield return new WaitForSeconds(0.2f);   // let first-frame hitches settle
#if UNITY_WEBGL && !UNITY_EDITOR
        NotifyGameReady();
#else
        Debug.Log("[GameReadySignal] game-ready -> parent (no-op in editor)");
#endif
    }
}
