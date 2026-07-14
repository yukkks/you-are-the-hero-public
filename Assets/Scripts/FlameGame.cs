using UnityEngine;

// The Last Flame loop (prototype scope): the room starts cave-dark, colleagues
// are stone, the player carries a full flame. Greeting a colleague wakes them
// (stone -> life) and warms the room one step; the carried flame dims by one
// step each time she gives it. When all are woken, the room is fully golden.
//
// Rides on the existing interaction spine: it just listens to GameProgress and
// reacts. Photos count toward warming too, so the whole room participates.
public class FlameGame : MonoBehaviour
{
    LightingDirector director;
    CarriedFlame flame;
    int totalSteps;

    void Start()
    {
        director = LightingDirector.Instance;
        flame = FindObjectOfType<CarriedFlame>();

        // put every colleague to sleep as stone at the start
        foreach (var npc in FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var stone = npc.GetComponent<StoneState>();
            if (stone == null) stone = npc.gameObject.AddComponent<StoneState>();
            stone.Sleep();
        }

        if (GameProgress.Instance != null)
            GameProgress.Instance.OnProgressChanged.AddListener(OnProgress);
        if (director != null) director.PreviewInstant(0f);   // start cave-dark
        if (flame != null) flame.SetFuel(1f);                // start with a full flame
        OnProgress();
    }

    void OnProgress()
    {
        var gp = GameProgress.Instance;
        if (gp == null) return;

        // count live: totals are only final once every interactable has
        // registered itself (their Start may run after ours)
        totalSteps = Mathf.Max(1, gp.NpcTotal + gp.PhotoTotal);
        int done = gp.GreetedCount + gp.PhotosViewedCount;
        float progress = Mathf.Clamp01(done / (float)totalSteps);

        // wake any colleague that was just greeted
        foreach (var npc in FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!gp.IsGreeted(npc.gameObject)) continue;
            var stone = npc.GetComponent<StoneState>();
            if (stone != null && !stone.IsAwake) stone.Wake();
        }

        // the room warms with progress; her flame spends down as she gives it,
        // but never fully out until the very last step (that's the story's beat)
        if (director != null) director.SetTarget(progress);
        if (flame != null) flame.SetFuel(Mathf.Lerp(1f, 0.12f, progress));
    }
}
