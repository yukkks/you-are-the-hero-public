// Anything the hero can walk up to and engage with (an NPC, a photo on the wall).
// Unifies every interaction behind one gesture: click it, the hero walks over, this fires.
public interface IInteractable
{
    // World position the hero should walk toward before interacting.
    UnityEngine.Vector3 InteractPosition { get; }

    // Called once the hero has arrived. Show the dialogue / unveil the photo / etc.
    void Interact();
}
