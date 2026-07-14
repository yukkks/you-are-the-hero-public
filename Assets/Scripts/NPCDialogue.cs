using UnityEngine;

// A colleague the hero can walk up to and greet. Click them -> the hero walks
// over (ClickToMove) -> their lines show one at a time (player clicks Next).
public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Who is this colleague?")]
    public string npcName = "Colleague";

    [Header("What they say (one entry = one screen; player clicks Next)")]
    [TextArea(2, 4)] public string[] lines = { "It's been an honour working alongside you." };

    [Header("Portrait shown in the dialogue panel (optional)")]
    public Sprite portrait;

    [Header("Their voice, played when the dialogue opens (optional)")]
    public AudioClip voiceClip;

    [Header("Where the hero stands to talk (optional)")]
    public Transform talkAnchor;   // falls back to this object's position

    public Vector3 InteractPosition => talkAnchor != null ? talkAnchor.position : transform.position;

    void Start()
    {
        if (GameProgress.Instance != null) GameProgress.Instance.RegisterNPC(gameObject);
    }

    public void Interact()
    {
        if (DialogueUI.Instance != null) DialogueUI.Instance.Show(npcName, lines, portrait);
        if (voiceClip != null) GameAudio.PlayVoice(voiceClip);
        bool newly = GameProgress.Instance != null && GameProgress.Instance.MarkGreeted(gameObject);
        if (newly)
        {
            GameAudio.PlayGreet();
            var presence = GetComponent<NPCPresence>();
            if (presence != null) presence.Bow();
        }
    }
}
