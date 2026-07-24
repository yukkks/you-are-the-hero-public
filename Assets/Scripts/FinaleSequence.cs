using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The cinematic finale — the moment the whole game builds to.
// Beat 1  the last goodbye lands; music falls silent; her flame gutters out;
//         the room drops to near-dark for one held breath.
// Beat 2  in that darkness everyone gathers at the tree (hidden by the dark).
// Beat 3  the tree ignites; the room floods warm; Tokyo lights up outside,
//         building by building. Confetti. The line appears.
// Beat 4  hand off to the web wrapper for the closing words.
public class FinaleSequence : MonoBehaviour
{
    [TextArea] public string line1 = "Every flame in this room was lit from yours.";
    [TextArea] public string line2 = "For eight years — you lit up this whole city.";
    [TextArea] public string line3 = "But the brightest things you leave behind are us.";

    bool played;

    void Start()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnAllComplete.AddListener(Begin);
    }

    void Begin()
    {
        if (played) return;
        played = true;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        var director = LightingDirector.Instance;
        var flame = FindObjectOfType<CarriedFlame>();
        var tree = GameObject.Find("/Lounge/TreeCentre");
        var city = GameObject.Find("/Lounge/Cityscape");

        // Take full control: close any open dialogue/photo, and stop FlameGame
        // from driving the room lighting (it would fight this sequence).
        if (DialogueUI.Instance != null) DialogueUI.Instance.Hide();
        var viewer = FindObjectOfType<PhotoViewerUI>();
        if (viewer != null) viewer.Close();
        var flameGame = FindObjectOfType<FlameGame>();
        if (flameGame != null) flameGame.enabled = false;
        yield return new WaitForSeconds(1.2f);   // let the last goodbye settle

        // ---- Beat 1: silence, the flame gutters out, the room breathes dark ----
        if (GameAudio.Instance != null) GameAudio.FadeOutMusic(1.2f);
        if (flame != null) StartCoroutine(Gutter(flame));
        if (director != null) director.SetTarget(0.05f);
        yield return new WaitForSeconds(2.2f);

        // ---- Beat 2: gather everyone at the tree while it's dark ----
        GatherAtTree(tree);
        yield return new WaitForSeconds(0.4f);

        // ---- Beat 3: ignition ----
        if (GameAudio.Instance != null) GameAudio.PlayCompletion();
        if (director != null) director.SetTarget(1f);           // room floods warm
        IgniteTree(tree);
        yield return StartCoroutine(LightUpCity(city));         // Tokyo, building by building
        SpawnConfetti(tree);

        // ---- the three lines, on the story overlay ----
        yield return ShowLine(line1, 3.2f);
        yield return ShowLine(line2, 3.6f);
        yield return ShowLine(line3, 4.2f);

        // ---- Beat 4: hand off to the web wrapper for the closing words ----
        var signal = FindObjectOfType<GameCompleteSignal>();
        if (signal != null) signal.Fire();
    }

    IEnumerator Gutter(CarriedFlame flame)
    {
        float t = 0f;
        while (t < 1.8f) { t += Time.deltaTime; flame.SetFuel(Mathf.Lerp(flame.fuel, 0f, t / 1.8f)); yield return null; }
        flame.SetFuel(0f);
    }

    void GatherAtTree(GameObject tree)
    {
        if (tree == null) return;
        Vector3 c = TreeCenter(tree);
        var npcs = new List<Transform>();
        var root = GameObject.Find("/Characters");
        if (root != null) foreach (Transform t in root.transform) if (t.gameObject.activeSelf) npcs.Add(t);
        // also bring the player into the ring
        var mover = FindObjectOfType<ClickToMove>();
        int n = npcs.Count + (mover != null ? 1 : 0);
        int i = 0;
        float radius = 3.6f;
        foreach (var t in npcs)
        {
            float a = (i++ / (float)n) * Mathf.PI * 2f;
            Vector3 p = c + new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * radius;
            p.y = t.position.y;
            t.position = p;
            t.rotation = Quaternion.LookRotation(new Vector3(c.x - p.x, 0, c.z - p.z));
            var pres = t.GetComponent<NPCPresence>();
            if (pres != null) pres.Bow();
        }
        if (mover != null)
        {
            float a = (i / (float)n) * Mathf.PI * 2f;
            Vector3 p = c + new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * radius;
            p.y = mover.transform.position.y;
            var agent = mover.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.Warp(p); else mover.transform.position = p;
            mover.transform.rotation = Quaternion.LookRotation(new Vector3(c.x - p.x, 0, c.z - p.z));
        }
    }

    void IgniteTree(GameObject tree)
    {
        if (tree == null) return;
        foreach (var r in tree.GetComponentsInChildren<MeshRenderer>())
        {
            foreach (var m in r.materials)
            {
                if (!m.HasProperty("_EmissionColor")) continue;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.35f) * 1.4f);
            }
        }
        var lightGo = new GameObject("TreeGlow");
        lightGo.transform.position = TreeCenter(tree) + Vector3.up * 1.5f;
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.72f, 0.4f);
        l.range = 14f; l.intensity = 3.5f; l.shadows = LightShadows.None;
    }

    IEnumerator LightUpCity(GameObject city)
    {
        if (city == null) yield break;
        var bldgs = new List<MeshRenderer>();
        foreach (Transform t in city.transform)
            if (t.name.StartsWith("Bldg")) bldgs.Add(t.GetComponent<MeshRenderer>());
        // shuffle-ish by index parity so it lights up scattered, not in a row
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = pass; i < bldgs.Count; i += 2)
            {
                var mr = bldgs[i]; if (mr == null) continue;
                foreach (var m in mr.materials)
                {
                    if (!m.HasProperty("_EmissionColor")) continue;
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.55f) * 0.9f);
                }
                if (i % 3 == 0) yield return new WaitForSeconds(0.06f);
            }
        }
    }

    void SpawnConfetti(GameObject tree)
    {
        var go = new GameObject("Confetti");
        go.transform.position = TreeCenter(tree) + Vector3.up * 3.5f;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.5f; main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.13f);
        main.gravityModifier = 0.5f;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.4f), new Color(0.95f, 0.45f, 0.55f));
        var em = ps.emission; em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 120), new ParticleSystem.Burst(0.4f, 90) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 45f; shape.radius = 1.5f;
        var psr = go.GetComponent<ParticleSystemRenderer>();
        if (PrimitiveLibrary.Particle != null) psr.material = PrimitiveLibrary.Particle;
        Destroy(go, 6f);
    }

    IEnumerator ShowLine(string text, float hold)
    {
        var overlay = StoryOverlay.Instance;
        if (overlay != null) { yield return overlay.Play(text, hold); }
        else yield return new WaitForSeconds(hold);
    }

    Vector3 TreeCenter(GameObject tree)
    {
        var rends = tree.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return tree.transform.position;
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        return new Vector3(b.center.x, b.min.y, b.center.z);
    }
}
