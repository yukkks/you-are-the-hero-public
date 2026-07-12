using UnityEngine;

// A little clickable memory: walk up, click, one warm line appears (and an
// optional sound plays). Doesn't count toward progress — pure delight.
public class EasterEgg : MonoBehaviour, IInteractable
{
    public string title = "A memory";
    [TextArea(2, 4)] public string[] lines = { "This spot meant something." };
    public AudioClip sound;
    public Transform standAnchor;

    public Vector3 InteractPosition => standAnchor != null ? standAnchor.position : transform.position;

    public void Interact()
    {
        if (DialogueUI.Instance != null) DialogueUI.Instance.Show(title, lines);
        if (sound != null) GameAudio.PlayClip(sound);
    }
}
