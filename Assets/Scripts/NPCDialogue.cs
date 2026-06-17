using UnityEngine;

// A colleague the hero can walk up to and greet. Click them -> the hero walks
// over (ClickToMove) -> this fires -> their line shows and progress is marked.
public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Who is this colleague?")]
    public string npcName = "Colleague";
    [TextArea(2, 5)] public string line = "It's been an honour working alongside you.";

    [Header("Where the hero stands to talk (optional)")]
    public Transform talkAnchor;   // falls back to this object's position

    public Vector3 InteractPosition => talkAnchor != null ? talkAnchor.position : transform.position;

    void Start()
    {
        if (GameProgress.Instance != null) GameProgress.Instance.RegisterNPC(gameObject);
    }

    public void Interact()
    {
        if (DialogueUI.Instance != null) DialogueUI.Instance.Show(npcName, line);
        if (GameProgress.Instance != null) GameProgress.Instance.MarkGreeted(gameObject);
    }
}
