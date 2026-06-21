using UnityEngine;
using UnityEngine.EventSystems;

// Smart third-person follow camera (in the spirit of "Messenger" by abeto):
//  - Drag (mouse or one finger) anywhere not on UI to orbit around the hero.
//  - When you move and aren't dragging, the camera auto-recenters behind you,
//    so you never have to fiddle with it.
// Movement is camera-relative (ClickToMove), so "where the camera faces" is forward.
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Orbit")]
    public float distance = 8f;
    public float lookHeight = 1.6f;
    public float yaw = 0f;
    public float pitch = 18f;
    public float minPitch = 6f;
    public float maxPitch = 65f;

    [Header("Drag to rotate")]
    public float rotateSpeed = 0.25f;     // degrees per pixel dragged
    public float dragThreshold = 8f;      // pixels before a press counts as a drag

    [Header("Smart auto-recenter")]
    public bool autoRecenter = true;
    public float recenterDelay = 1.0f;    // seconds after a drag before recentering kicks in
    public float recenterSpeed = 120f;    // degrees per second

    [Header("Smoothing")]
    public float followSmooth = 12f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float cameraRadius = 0.3f;
    public float minDistanceFromPlayer = 2f;

    Vector2 lastPointer;
    bool pressing, draggingThisPress;
    float lastDragTime = -99f;
    Vector3 lastTargetPos;

    void Start()
    {
        if (target != null) lastTargetPos = target.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressing = !IsPointerOverUI();   // ignore presses that start on UI (e.g. the joystick)
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

        // Smart recenter: when moving and not recently dragged, ease yaw to behind the hero.
        if (autoRecenter && Time.time - lastDragTime > recenterDelay)
        {
            if ((target.position - lastTargetPos).sqrMagnitude > 0.0000001f)
                yaw = Mathf.MoveTowardsAngle(yaw, target.eulerAngles.y, recenterSpeed * Time.deltaTime);
        }
        lastTargetPos = target.position;

        Vector3 pivot = target.position + Vector3.up * lookHeight;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = pivot + rot * new Vector3(0f, 0f, -distance);

        Vector3 dir = desired - pivot;
        float dist = dir.magnitude; dir.Normalize();
        Vector3 finalPos = desired;
        if (Physics.SphereCast(pivot, cameraRadius, dir, out RaycastHit hit, dist, collisionLayers))
        {
            float adj = Mathf.Max(hit.distance - cameraRadius, minDistanceFromPlayer);
            finalPos = pivot + dir * adj;
        }

        transform.position = Vector3.Lerp(transform.position, finalPos, followSmooth * Time.deltaTime);
        transform.LookAt(pivot);
    }

    static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
