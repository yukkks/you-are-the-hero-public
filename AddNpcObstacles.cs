using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

// One-off: give each colleague a carving NavMeshObstacle so the hero (a
// NavMeshAgent) can't walk through them. Sized in world units regardless of the
// avatar's transform scale. Keeps their CapsuleCollider (used for click-to-greet).
public static class AddNpcObstacles
{
    static readonly string[] Npcs = {
        "Characters/YuAttia", "Characters/Rebecca", "Characters/Paul",
        "Characters/Shikha", "Characters/model-14"
    };

    public static string Execute()
    {
        int done = 0;
        foreach (var path in Npcs)
        {
            var go = GameObject.Find(path);
            if (go == null) continue;

            var obs = go.GetComponent<NavMeshObstacle>();
            if (obs == null) obs = go.AddComponent<NavMeshObstacle>();

            Vector3 ls = go.transform.lossyScale;
            float xz = Mathf.Max(0.0001f, Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.z)));
            float y  = Mathf.Max(0.0001f, Mathf.Abs(ls.y));

            obs.shape = NavMeshObstacleShape.Capsule;
            obs.radius = 0.45f / xz;          // ~0.45 m world footprint
            obs.height = 2.0f  / y;           // ~2 m tall
            obs.center = new Vector3(0f, 1.0f / y, 0f);
            obs.carving = true;
            obs.carveOnlyStationary = true;

            EditorUtility.SetDirty(obs);
            done++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return "Added/updated carving NavMeshObstacle on " + done + " colleagues.";
    }
}
