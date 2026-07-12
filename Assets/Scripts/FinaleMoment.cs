using UnityEngine;

// The in-game beat when everything is complete, before the web wrapper takes
// over: confetti over the hero, every colleague bows together, and the
// progress pill becomes a farewell line. (GameCompleteSignal still fires
// separately — the wrapper owns the real ending.)
public class FinaleMoment : MonoBehaviour
{
    [TextArea] public string completeLine = "Everyone came to say goodbye.";

    void Start()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnAllComplete.AddListener(Play);
    }

    void Play()
    {
        // every colleague bows in unison
        foreach (var p in FindObjectsByType<NPCPresence>(FindObjectsSortMode.None))
            p.Bow();

        // confetti above the hero
        var mover = FindObjectOfType<ClickToMove>();
        Vector3 pos = mover != null ? mover.transform.position + Vector3.up * 2.6f : transform.position;
        SpawnConfetti(pos);

        // the progress pill becomes the farewell line
        var nudge = FindObjectOfType<ProgressNudgeUI>();
        if (nudge != null && nudge.label != null) nudge.label.text = completeLine;
    }

    void SpawnConfetti(Vector3 pos)
    {
        var go = new GameObject("Confetti");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.2f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.10f);
        main.gravityModifier = 0.6f;
        main.startRotation3D = true;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.78f, 0.35f), new Color(0.95f, 0.4f, 0.55f));
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 90), new ParticleSystem.Burst(0.35f, 70) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 42f;
        shape.radius = 0.25f;
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        var psr = go.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        Destroy(go, 5f);
    }
}
