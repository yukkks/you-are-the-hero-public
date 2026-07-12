using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// The spine of the experience. Tracks which colleagues the hero has greeted and
// which memories (photos) she has unveiled. When everything is done, fires
// OnAllComplete -> the in-lounge gathering finale.
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    // Interactables register themselves so totals are derived, never hand-counted.
    // Keyed by the objects themselves (GetInstanceID is a compile error on newer Unity).
    private readonly HashSet<GameObject> npcs = new HashSet<GameObject>();
    private readonly HashSet<GameObject> npcsGreeted = new HashSet<GameObject>();
    private readonly HashSet<GameObject> photos = new HashSet<GameObject>();
    private readonly HashSet<GameObject> photosViewed = new HashSet<GameObject>();

    public UnityEvent OnProgressChanged = new UnityEvent();   // drives the on-screen nudge
    public UnityEvent OnAllComplete = new UnityEvent();        // triggers the finale

    private bool completed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterNPC(GameObject npc) { npcs.Add(npc); }
    public void RegisterPhoto(GameObject photo) { photos.Add(photo); }

    // Both return true only the FIRST time (drives one-time chimes/bows).
    public bool MarkGreeted(GameObject npc)
    {
        bool newly = npcsGreeted.Add(npc);
        if (newly) Changed();
        return newly;
    }

    public bool MarkPhotoViewed(GameObject photo)
    {
        bool newly = photosViewed.Add(photo);
        if (newly) Changed();
        return newly;
    }

    public int GreetedCount => npcsGreeted.Count;
    public int NpcTotal => npcs.Count;
    public int PhotosViewedCount => photosViewed.Count;
    public int PhotoTotal => photos.Count;

    public bool IsComplete()
    {
        return NpcTotal > 0 && PhotoTotal > 0
            && npcsGreeted.Count >= npcs.Count
            && photosViewed.Count >= photos.Count;
    }

    void Changed()
    {
        OnProgressChanged.Invoke();
        if (!completed && IsComplete())
        {
            completed = true;
            OnAllComplete.Invoke();
        }
    }
}
