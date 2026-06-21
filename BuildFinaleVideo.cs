using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.Events;

// One-off: build the full-screen finale video playback rig, wire it, switch the
// finale over from the real-time cutscene to the video, and point the Skip button
// at it. Drop the clip at Assets/StreamingAssets/finale.mp4.
public static class BuildFinaleVideo
{
    public static string Execute()
    {
        // folders
        if (!AssetDatabase.IsValidFolder("Assets/Video")) AssetDatabase.CreateFolder("Assets", "Video");
        if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets")) AssetDatabase.CreateFolder("Assets", "StreamingAssets");

        // render texture (portrait, matches the game)
        const string rtPath = "Assets/Video/FinaleRT.renderTexture";
        var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (rt == null)
        {
            rt = new RenderTexture(1080, 1920, 0) { name = "FinaleRT" };
            AssetDatabase.CreateAsset(rt, rtPath);
        }

        var canvas = GameObject.Find("Canvas").transform;

        // full-screen RawImage that shows the video
        var oldScreen = canvas.Find("FinaleScreen");
        if (oldScreen != null) Object.DestroyImmediate(oldScreen.gameObject);
        var screenGo = new GameObject("FinaleScreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        screenGo.transform.SetParent(canvas, false);
        var srt = (RectTransform)screenGo.transform;
        srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one; srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
        var raw = screenGo.GetComponent<RawImage>();
        raw.texture = rt; raw.color = Color.white;
        screenGo.SetActive(false);

        // playback rig
        var oldRig = GameObject.Find("FinaleVideoRig");
        if (oldRig != null) Object.DestroyImmediate(oldRig);
        var rig = new GameObject("FinaleVideoRig", typeof(VideoPlayer), typeof(AudioSource), typeof(FinaleVideo));
        var vp = rig.GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rt;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        var aud = rig.GetComponent<AudioSource>();
        aud.playOnAwake = false;

        var fv = rig.GetComponent<FinaleVideo>();
        fv.player = vp;
        fv.audioSource = aud;
        fv.screen = screenGo;
        fv.videoFileName = "finale.mp4";
        fv.playerControl = GameObject.Find("Player").GetComponent<ClickToMove>();
        fv.joystick = canvas.Find("MoveJoystick").gameObject;
        fv.cameraRig = GameObject.Find("Main Camera").GetComponent<CameraFollow>();
        fv.hideOnFinale = new GameObject[] {
            canvas.Find("ProgressNudge").gameObject,
            canvas.Find("DialoguePanel").gameObject,
        };

        // switch the finale over: disable the real-time cutscene controller
        var fc = GameObject.Find("GameManager").GetComponent<FinaleController>();
        fc.enabled = false;

        // point the Skip button at the video instead of the real-time finale
        var btn = canvas.Find("SkipFinaleButton").GetComponent<Button>();
        UnityEventTools.RemovePersistentListener(btn.onClick, fc.TriggerFinale);
        UnityEventTools.AddPersistentListener(btn.onClick, fv.Play);

        EditorUtility.SetDirty(fv);
        EditorUtility.SetDirty(btn);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Finale video rig built + wired; real-time finale disabled; Skip points to video. Drop Assets/StreamingAssets/finale.mp4.";
    }
}
