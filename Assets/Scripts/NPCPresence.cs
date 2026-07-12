using System.Collections;
using UnityEngine;

// Makes a colleague feel present: they turn to face the hero when she comes
// near, drift back to their original pose when she leaves, and give a small
// bow the first time she greets them.
public class NPCPresence : MonoBehaviour
{
    public float faceDistance = 3.5f;
    public float turnSpeed = 3.5f;

    Transform hero;
    Quaternion restRotation;
    Coroutine bowRoutine;
    float bowAngle;   // current forward-lean applied on top of facing

    void Start()
    {
        restRotation = transform.rotation;
        var mover = FindObjectOfType<ClickToMove>();
        if (mover) hero = mover.transform;
    }

    void LateUpdate()
    {
        if (hero == null) return;

        Vector3 to = hero.position - transform.position;
        to.y = 0f;
        Quaternion target;
        if (to.sqrMagnitude < faceDistance * faceDistance && to.sqrMagnitude > 0.01f)
            target = Quaternion.LookRotation(to);
        else
            target = restRotation;

        Quaternion faced = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
        transform.rotation = faced * Quaternion.AngleAxis(bowAngle, Vector3.right);
    }

    public void Bow()
    {
        if (bowRoutine != null) StopCoroutine(bowRoutine);
        bowRoutine = StartCoroutine(BowRoutine());
    }

    IEnumerator BowRoutine()
    {
        // lean forward 12° and back — a small, warm greeting bow
        float t = 0f;
        const float down = 0.35f, hold = 0.25f, up = 0.5f;
        while (t < down) { t += Time.deltaTime; bowAngle = Mathf.SmoothStep(0f, 12f, t / down); yield return null; }
        yield return new WaitForSeconds(hold);
        t = 0f;
        while (t < up) { t += Time.deltaTime; bowAngle = Mathf.SmoothStep(12f, 0f, t / up); yield return null; }
        bowAngle = 0f;
        bowRoutine = null;
    }
}
