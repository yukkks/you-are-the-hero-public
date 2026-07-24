using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

// Hero control:
//  - Free-roam movement: on-screen VirtualJoystick (for the recipient on phone/web)
//    OR keyboard arrows/WASD (desktop testing). Both are camera-relative and analog.
//  - Tap a colleague / photo and the hero auto-walks over and engages.
// Everything runs through the NavMeshAgent, so the hero always respects walls/furniture.
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    [Header("References")]
    public Camera viewCamera;                 // defaults to Camera.main
    public Animator characterAnimator;        // optional; auto-found in children (for the Avaturn avatar later)

    [Header("Keyboard (desktop)")]
    public bool enableKeyboard = true;        // arrow keys / WASD
    public float turnSmoothing = 12f;

    [Header("Interaction")]
    public float interactStopDistance = 1.8f; // how close the hero stops before engaging

    [Header("Click filtering")]
    public LayerMask clickMask = ~0;          // what the click ray can hit
    public float tapThreshold = 12f;          // px; a press that moves more than this is a camera drag, not a tap

    private NavMeshAgent agent;
    private IInteractable pendingTarget;
    private Vector2 pressPos;
    private bool pressValid;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;         // we face the hero ourselves (smoother for both schemes)
        agent.stoppingDistance = 0f;

        if (viewCamera == null) viewCamera = Camera.main;
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // While a colleague is speaking, freeze movement entirely — the joystick
        // is hidden and taps belong to the dialogue, not the floor.
        if (DialogueUI.IsOpen)
        {
            if (agent.hasPath) agent.ResetPath();
            UpdateAnimator(false);
            return;
        }

        Vector3 dir = GetDirectionalInput();      // joystick or keyboard, camera-relative
        bool moving = dir.sqrMagnitude > 0.0001f;

        HandleTap();   // a quick tap interacts; a drag is left for the camera to orbit

        if (moving)
        {
            // free-roam takes over: cancel any tap-to-walk destination
            pendingTarget = null;
            if (agent.hasPath) agent.ResetPath();
            agent.Move(dir * agent.speed * Time.deltaTime);
            FaceDirection(dir);
        }
        else
        {
            CheckArrival();
            FacePathDirection();
        }

        UpdateAnimator(moving);
    }

    // ---------- Free-roam directional input (joystick + keyboard), camera-relative ----------
    Vector3 GetDirectionalInput()
    {
        if (agent == null) return Vector3.zero;

        float h = 0f, v = 0f;
        if (enableKeyboard)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }
        if (VirtualJoystick.Instance != null)
        {
            Vector2 j = VirtualJoystick.Instance.Direction;
            if (j.sqrMagnitude > 0.0001f) { h = j.x; v = j.y; }   // joystick overrides keyboard when active
        }

        Vector3 raw = new Vector3(h, 0f, v);
        if (raw.sqrMagnitude < 0.01f) return Vector3.zero;
        if (raw.sqrMagnitude > 1f) raw.Normalize();               // keep analog magnitude, cap at 1

        Vector3 camF = viewCamera != null ? viewCamera.transform.forward : Vector3.forward;
        Vector3 camR = viewCamera != null ? viewCamera.transform.right : Vector3.right;
        camF.y = 0f; camR.y = 0f; camF.Normalize(); camR.Normalize();
        return camF * raw.z + camR * raw.x;                       // analog: magnitude scales speed
    }

    // ---------- Tap detection (distinguishes a tap from a camera drag) ----------
    void HandleTap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressValid = !PointerIsOverUI();   // ignore presses that start on UI (joystick, dialogue)
            pressPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (pressValid && ((Vector2)Input.mousePosition - pressPos).magnitude < tapThreshold)
                HandleClick();
            pressValid = false;
        }
    }

    // ---------- Tap to walk-and-engage ----------
    void HandleClick()
    {
        if (viewCamera == null) return;

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 300f, clickMask)) return;
        if (hit.collider.transform.root == transform) return; // ignore clicks on the hero

        var interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            // Walk to a spot IN FRONT of the target (on the hero's approach side),
            // not onto it — otherwise she ends up standing inside/behind them and
            // the follow camera tangles. Offset from the target toward the hero.
            pendingTarget = interactable;
            Vector3 ipos = interactable.InteractPosition;
            Vector3 toHero = transform.position - ipos; toHero.y = 0f;
            Vector3 stand = ipos + (toHero.sqrMagnitude > 0.01f ? toHero.normalized : -transform.forward) * interactStopDistance;
            if (NavMesh.SamplePosition(stand, out NavMeshHit sHit, 2f, NavMesh.AllAreas)) stand = sHit.position;
            agent.stoppingDistance = 0.1f;   // we already offset to the front
            agent.SetDestination(stand);
            TapRipple.Spawn(stand);
            return;
        }

        // tap-to-go: tap any walkable spot and the hero strolls there — the
        // primary control for a non-gamer on a phone (joystick stays available)
        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.5f, NavMesh.AllAreas))
        {
            pendingTarget = null;
            agent.stoppingDistance = 0f;
            agent.SetDestination(navHit.position);
            TapRipple.Spawn(navHit.position);
        }
    }

    void CheckArrival()
    {
        if (pendingTarget == null || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.15f)
        {
            var target = pendingTarget;
            pendingTarget = null;
            agent.stoppingDistance = 0f;

            Vector3 look = target.InteractPosition - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);

            target.Interact();
        }
    }

    // ---------- Facing ----------
    void FacePathDirection()
    {
        Vector3 v = agent.velocity; v.y = 0f;
        if (v.sqrMagnitude > 0.05f) FaceDirection(v.normalized);
    }

    void FaceDirection(Vector3 dir)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), turnSmoothing * Time.deltaTime);
    }

    void UpdateAnimator(bool moving)
    {
        if (characterAnimator == null) return;
        characterAnimator.SetBool("isMoving", moving || agent.velocity.sqrMagnitude > 0.05f);
    }

    static bool PointerIsOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
