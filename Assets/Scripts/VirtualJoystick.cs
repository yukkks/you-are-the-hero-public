using UnityEngine;
using UnityEngine.EventSystems;

// On-screen thumbstick for free-roam movement (touch + mouse). Drag the knob;
// ClickToMove reads Direction each frame and walks the hero that way (camera-relative).
// Self-registers as a singleton so ClickToMove needs no inspector wiring.
[RequireComponent(typeof(RectTransform))]
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static VirtualJoystick Instance { get; private set; }

    [Tooltip("The movable knob (a child RectTransform).")]
    public RectTransform handle;
    [Tooltip("How far the knob can travel from center, in pixels.")]
    public float handleRange = 90f;
    [Tooltip("Inputs smaller than this are ignored (dead zone).")]
    public float deadZone = 0.15f;

    RectTransform baseRect;
    Vector2 input = Vector2.zero;

    // Normalized -1..1 per axis; zero inside the dead zone.
    public Vector2 Direction => input.magnitude < deadZone ? Vector2.zero : input;

    void Awake()
    {
        Instance = this;
        baseRect = GetComponent<RectTransform>();
    }

    void OnDisable() { ResetKnob(); }

    public void OnPointerDown(PointerEventData e) { OnDrag(e); }

    public void OnDrag(PointerEventData e)
    {
        if (baseRect == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                baseRect, e.position, e.pressEventCamera, out Vector2 pos)) return;

        Vector2 v = pos / Mathf.Max(1f, handleRange);
        input = v.magnitude > 1f ? v.normalized : v;
        if (handle) handle.anchoredPosition = input * handleRange;
    }

    public void OnPointerUp(PointerEventData e) { ResetKnob(); }

    void ResetKnob()
    {
        input = Vector2.zero;
        if (handle) handle.anchoredPosition = Vector2.zero;
    }
}
