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
    private readonly HashSet<int> npcs = new HashSet<int>();
    private readonly HashSet<int> npcsGreeted = new HashSet<int>();
    private readonly HashSet<int> photos = new HashSet<int>();
    private readonly HashSet<int> photosViewed = new HashSet<int>();

    public UnityEvent OnProgressChanged = new UnityEvent();   // drives the on-screen nudge
    public UnityEvent OnAllComplete = new UnityEvent();        // triggers the finale

    private bool completed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterNPC(GameObject npc) { npcs.Add(npc.GetInstanceID()); }
    public void RegisterPhoto(GameObject photo) { photos.Add(photo.GetInstanceID()); }

    public void MarkGreeted(GameObject npc)
    {
        if (npcsGreeted.Add(npc.GetInstanceID())) Changed();
    }

    public void MarkPhotoViewed(GameObject photo)
    {
        if (photosViewed.Add(photo.GetInstanceID())) Changed();
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
