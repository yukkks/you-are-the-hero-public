using UnityEngine;
using TMPro;

// A colleague's name floating softly above their head. Billboards to the
// camera and fades in as the hero approaches (touch-friendly wayfinding).
public class NameTag : MonoBehaviour
{
    public string displayName = "";
    public float showDistance = 7f;
    public float height = 0.12f;   // above the beacon anchor / head

    TextMeshPro text;
    Transform hero;
    float topY;

    void Start()
    {
        var npc = GetComponent<NPCDialogue>();
        if (string.IsNullOrEmpty(displayName) && npc != null) displayName = npc.npcName;

        var rends = GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(transform.position + Vector3.up * 1.7f, Vector3.one * 0.2f);
        bool first = true;
        foreach (var r in rends)
        {
            if (r.GetComponent<ParticleSystem>() != null) continue;
            if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
        }
        topY = b.max.y + height;

        var go = new GameObject("NameTag");
        go.transform.SetParent(transform, true);
        go.transform.position = new Vector3(b.center.x, topY, b.center.z);
        text = go.AddComponent<TextMeshPro>();
        text.text = displayName;
        text.fontSize = 2.4f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.93f, 0.78f, 0f);
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color32(30, 20, 10, 200);
        var rt = text.rectTransform;
        rt.sizeDelta = new Vector2(4f, 1f);

        var mover = FindObjectOfType<ClickToMove>();
        if (mover) hero = mover.transform;
    }

    void LateUpdate()
    {
        if (text == null) return;
        var cam = Camera.main;
        if (cam != null)
            text.transform.rotation = Quaternion.LookRotation(text.transform.position - cam.transform.position);

        float target = 0f;
        if (hero != null)
        {
            float d = Vector3.Distance(hero.position, transform.position);
            target = Mathf.InverseLerp(showDistance, showDistance * 0.55f, d);   // 0 far -> 1 near
        }
        Color c = text.color;
        c.a = Mathf.MoveTowards(c.a, target, Time.deltaTime * 3f);
        text.color = c;
    }
}
