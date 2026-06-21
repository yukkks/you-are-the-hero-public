using UnityEngine;

// A colleague the hero can walk up to and greet. Click them -> the hero walks
// over (ClickToMove) -> their lines show one at a time (player clicks Next).
public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Who is this colleague?")]
    public string npcName = "Colleague";

    [Header("What they say (one entry = one screen; player clicks Next)")]
    [TextArea(2, 4)] public string[] lines = { "It's been an honour working alongside you." };

    [Header("Where the hero stands to talk (optional)")]
    public Transform talkAnchor;   // falls back to this object's position

    public Vector3 InteractPosition => talkAnchor != null ? talkAnchor.position : transform.position;

    void Start()
    {
        if (GameProgress.Instance != null) GameProgress.Instance.RegisterNPC(gameObject);
    }

    public void Interact()
    {
        if (DialogueUI.Instance != null) DialogueUI.Instance.Show(npcName, lines);
        if (GameProgress.Instance != null) GameProgress.Instance.MarkGreeted(gameObject);
    }
}
