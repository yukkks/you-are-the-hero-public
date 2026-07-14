using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

// First-person view: the hero IS the player, so she is never rendered.
// Camera rides at eye height on the player root. Drag (finger or mouse) to
// look around; while walking, the view eases back to the walk direction so a
// non-gamer never has to manage the camera. Tap-to-move works unchanged
// (ClickToMove raycasts from Camera.main).
public class FirstPersonCam : MonoBehaviour
{
    public Transform target;
    // The lounge and avatars are built at ~2x real-world scale (NPC heads sit
    // ~3.2m above the floor), so "eye height" must match that scale or the
    // first-person view stares at everyone's waist.
    public float eyeHeight = 3.15f;

    [Header("Drag to look")]
    public float rotateSpeed = 0.22f;     // degrees per pixel
    public float dragThreshold = 8f;
    public float minPitch = -35f;
    public float maxPitch = 55f;

    [Header("Auto-align (only while auto-walking to a tapped target)")]
    public bool autoAlign = true;
    public float alignSpeed = 160f;

    float yaw, pitch;
    Vector2 lastPointer;
    bool pressing, draggingThisPress;
    float lastDragTime = -99f;
    NavMeshAgent agentRef;

    void OnEnable()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            agentRef = target.GetComponent<NavMeshAgent>();
        }
        pitch = 8f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressing = !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
            draggingThisPress = false;
            lastPointer = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && pressing)
        {
            Vector2 cur = Input.mousePosition;
            Vector2 delta = cur - lastPointer;
            if (!draggingThisPress && delta.magnitude >= dragThreshold) draggingThisPress = true;
            if (draggingThisPress)
            {
                yaw += delta.x * rotateSpeed;
                pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed, minPitch, maxPitch);
                lastDragTime = Time.time;
            }
            lastPointer = cur;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            pressing = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // The camera is the authority: joystick movement is camera-relative and
        // NEVER swings the view (that was the swimmy feel). Only when the hero
        // auto-walks to a tapped colleague/photo does the view ease around to
        // face the walk direction, so she isn't walking blind.
        bool pathWalking = agentRef != null && agentRef.hasPath && agentRef.velocity.sqrMagnitude > 0.3f;
        if (autoAlign && pathWalking && Time.time - lastDragTime > 0.35f)
        {
            Vector3 v = agentRef.velocity; v.y = 0f;
            if (v.sqrMagnitude > 0.01f)
            {
                float walkYaw = Quaternion.LookRotation(v.normalized).eulerAngles.y;
                yaw = Mathf.MoveTowardsAngle(yaw, walkYaw, alignSpeed * Time.deltaTime);
                pitch = Mathf.MoveTowards(pitch, 8f, 25f * Time.deltaTime);
            }
        }

        transform.position = target.position + Vector3.up * eyeHeight;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
