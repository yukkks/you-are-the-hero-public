using UnityEngine;
using UnityEngine.AI;

// Procedural walk for the primitive silhouette figure: swings the legs and
// arms and bobs the body based on how fast she's actually moving. No rig, no
// animation clips — cheap and reads beautifully for a stylized silhouette.
public class SilhouetteWalk : MonoBehaviour
{
    public Transform body, head, lLeg, rLeg, lArm, rArm;
    public float strideSpeed = 8f;     // cycle speed per unit of movement
    public float legSwing = 32f;       // degrees
    public float armSwing = 26f;
    public float bob = 0.06f;

    NavMeshAgent agent;
    float phase;
    Vector3 bodyRest, headRest;

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        if (body) bodyRest = body.localPosition;
        if (head) headRest = head.localPosition;
        AutoBind();
    }

    void AutoBind()
    {
        if (body == null) body = transform.Find("Body");
        if (head == null) head = transform.Find("Head");
        if (lLeg == null) lLeg = transform.Find("LLeg");
        if (rLeg == null) rLeg = transform.Find("RLeg");
        if (lArm == null) lArm = transform.Find("LArm");
        if (rArm == null) rArm = transform.Find("RArm");
    }

    void Update()
    {
        float speed = agent != null ? new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude : 0f;
        float t = Mathf.Clamp01(speed / 5f);   // 0 idle .. 1 full walk

        if (speed > 0.15f) phase += Time.deltaTime * strideSpeed * (0.4f + t);
        float s = Mathf.Sin(phase);
        float s2 = Mathf.Sin(phase * 2f);

        // legs & arms swing opposite; scale by how fast she's going
        if (lLeg) lLeg.localRotation = Quaternion.Euler(s * legSwing * t, 0, 0);
        if (rLeg) rLeg.localRotation = Quaternion.Euler(-s * legSwing * t, 0, 0);
        if (lArm) lArm.localRotation = Quaternion.Euler(-s * armSwing * t, 0, 0);
        if (rArm) rArm.localRotation = Quaternion.Euler(s * armSwing * t, 0, 0);

        // gentle vertical bob at twice the stride; plus a breathing idle when still
        float idle = (1f - t) * Mathf.Sin(Time.time * 1.6f) * 0.012f;
        float step = Mathf.Abs(s2) * bob * t;
        if (body) body.localPosition = bodyRest + Vector3.up * (step + idle);
        if (head) head.localPosition = headRest + Vector3.up * (step + idle);
    }
}
