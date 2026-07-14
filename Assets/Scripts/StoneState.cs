using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Puts a colleague to "sleep" as cold stone and wakes them at the player's
// flame: caches their real materials, swaps to a grey stone set, freezes the
// animator; Wake() restores everything with a brief warm emissive pulse.
// Material-swap + animator-toggle only — proven, build-safe, no new tech.
public class StoneState : MonoBehaviour
{
    public Color stoneColor = new Color(0.44f, 0.46f, 0.50f);
    public float wakePulse = 1.2f;   // seconds of emissive bloom on waking

    readonly List<Renderer> rends = new List<Renderer>();
    readonly List<Material[]> realMats = new List<Material[]>();
    Material stoneMat;
    Animator anim;
    bool awoken;

    public bool IsAwake => awoken;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        stoneMat.color = stoneColor;
        stoneMat.SetFloat("_Smoothness", 0.08f);

        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponent<ParticleSystem>() != null) continue;
            rends.Add(r);
            realMats.Add(r.sharedMaterials);
        }
    }

    public void Sleep()
    {
        awoken = false;
        if (anim != null) anim.speed = 0f;   // freeze mid-pose
        for (int i = 0; i < rends.Count; i++)
        {
            var slots = new Material[rends[i].sharedMaterials.Length];
            for (int s = 0; s < slots.Length; s++) slots[s] = stoneMat;
            rends[i].sharedMaterials = slots;
        }
    }

    public void Wake()
    {
        if (awoken) return;
        awoken = true;
        if (anim != null) anim.speed = 1f;
        for (int i = 0; i < rends.Count; i++) rends[i].sharedMaterials = realMats[i];
        StartCoroutine(WarmPulse());
    }

    IEnumerator WarmPulse()
    {
        // restored materials get a short emissive bloom so waking reads as "warming"
        var glowing = new List<Material>();
        foreach (var r in rends)
            foreach (var m in r.materials)   // instances, safe to tweak
                if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); glowing.Add(m); }

        float t = 0f;
        while (t < wakePulse)
        {
            t += Time.deltaTime;
            float e = Mathf.Sin(t / wakePulse * Mathf.PI) * 0.6f;   // 0 -> peak -> 0
            foreach (var m in glowing) m.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.4f) * e);
            yield return null;
        }
        foreach (var m in glowing) m.SetColor("_EmissionColor", Color.black);
    }
}
