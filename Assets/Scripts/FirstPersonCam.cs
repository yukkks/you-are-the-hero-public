using UnityEngine;
using UnityEngine.EventSystems;

// First-person view: the hero IS the player, so she is never rendered.
// Camera rides at eye height on the player root. Drag (finger or mouse) to
// look around; while walking, the view eases back to the walk direction so a
// non-gamer never has to manage the camera. Tap-to-move works unchanged
// (ClickToMove raycasts from Camera.main).
public class FirstPersonCam : MonoBehaviour
{
    public Transform target;
    public float eyeHeight = 1.55f;

    [Header("Drag to look")]
    public float rotateSpeed = 0.22f;     // degrees per pixel
    public float dragThreshold = 8f;
    public float minPitch = -35f;
    public float maxPitch = 55f;

    [Header("Auto-align while walking")]
    public bool autoAlign = true;
    public float alignDelay = 1.0f;
    public float alignSpeed = 110f;

    float yaw, pitch;
    Vector2 lastPointer;
    bool pressing, draggingThisPress;
    float lastDragTime = -99f;
    Vector3 lastTargetPos;

    void OnEnable()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            lastTargetPos = target.position;
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

        bool moving = (target.position - lastTargetPos).sqrMagnitude > 0.0000001f;
        lastTargetPos = target.position;

        if (autoAlign && moving && Time.time - lastDragTime > alignDelay)
        {
            yaw = Mathf.MoveTowardsAngle(yaw, target.eulerAngles.y, alignSpeed * Time.deltaTime);
            pitch = Mathf.MoveTowards(pitch, 8f, 20f * Time.deltaTime);
        }

        transform.position = target.position + Vector3.up * eyeHeight;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
