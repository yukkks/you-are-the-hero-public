using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Delete unused (non-build) assets inside known previous-game / vendor-demo
// folders, to speed up the cloud build. Never deletes a build dependency.
// Recoverable via git.
public static class CleanupUnused
{
    static readonly string[] PurgeRoots = {
        "Assets/Scenes/",
        "Assets/Polytope Studio/",
        "Assets/YughuesFreeFlooringMaterials/",
        "Assets/StarterAssets/",
        "Assets/TutorialInfo/",
        "Assets/Cozy Furniture",
        "Assets/Waldemarst/",
    };

    public static string Execute()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;

        // build dependency set — never delete any of these
        var keep = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (!s.enabled) continue;
            keep.Add(s.path);
            foreach (var d in AssetDatabase.GetDependencies(s.path, true)) keep.Add(d);
        }

        long freed = 0; int count = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var p in AssetDatabase.GetAllAssetPaths())
            {
                if (!p.StartsWith("Assets/")) continue;
                if (keep.Contains(p)) continue;
                if (AssetDatabase.IsValidFolder(p)) continue;
                if (p.EndsWith(".cs") || p.EndsWith(".shader") || p.EndsWith(".asmdef")
                    || p.EndsWith(".asmref") || p.EndsWith(".hlsl") || p.EndsWith(".shadergraph")) continue;
                if (p.Contains("/Resources/") || p.Contains("/Editor/")) continue;

                bool inPurge = false;
                foreach (var r in PurgeRoots) if (p.StartsWith(r)) { inPurge = true; break; }
                if (!inPurge) continue;

                string abs = Path.Combine(root, p);
                if (File.Exists(abs)) freed += new FileInfo(abs).Length;
                if (AssetDatabase.DeleteAsset(p)) count++;
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }

        AssetDatabase.Refresh();
        return "Deleted " + count + " unused assets; freed ~"
            + (freed / 1024.0 / 1024.0).ToString("0") + " MB.";
    }
}
