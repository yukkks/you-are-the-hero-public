using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

// Hero control with two schemes that coexist:
//  - Click-to-move (for the non-gamer recipient): click the floor to walk there,
//    or click a colleague / photo and the hero walks over and engages automatically.
//  - Keyboard (for desktop testing): arrow keys or WASD, camera-relative.
// Both run through the NavMeshAgent, so the hero always respects walls and furniture.
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

    private NavMeshAgent agent;
    private IInteractable pendingTarget;

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
        bool movedByKeyboard = HandleKeyboard();

        if (!movedByKeyboard)
        {
            if (Input.GetMouseButtonDown(0) && !PointerIsOverUI())
                HandleClick();

            CheckArrival();
            FacePathDirection();
        }

        UpdateAnimator(movedByKeyboard);
    }

    // ---------- Keyboard (arrow keys / WASD), camera-relative ----------
    bool HandleKeyboard()
    {
        if (!enableKeyboard || agent == null) return false;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        var input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude < 0.01f) return false;

        Vector3 camF = viewCamera != null ? viewCamera.transform.forward : Vector3.forward;
        Vector3 camR = viewCamera != null ? viewCamera.transform.right : Vector3.right;
        camF.y = 0f; camR.y = 0f; camF.Normalize(); camR.Normalize();
        Vector3 dir = (camF * v + camR * h).normalized;

        // taking manual control cancels any click destination
        pendingTarget = null;
        if (agent.hasPath) agent.ResetPath();

        agent.Move(dir * agent.speed * Time.deltaTime);
        FaceDirection(dir);
        return true;
    }

    // ---------- Click-to-move ----------
    void HandleClick()
    {
        if (viewCamera == null) return;

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 300f, clickMask)) return;
        if (hit.collider.transform.root == transform) return; // ignore clicks on the hero

        var interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            pendingTarget = interactable;
            agent.stoppingDistance = interactStopDistance;
            agent.SetDestination(interactable.InteractPosition);
        }
        else
        {
            pendingTarget = null;
            agent.stoppingDistance = 0f;
            agent.SetDestination(hit.point);
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

    void UpdateAnimator(bool movedByKeyboard)
    {
        if (characterAnimator == null) return;
        bool moving = movedByKeyboard || agent.velocity.sqrMagnitude > 0.05f;
        characterAnimator.SetBool("isMoving", moving);
    }

    static bool PointerIsOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
