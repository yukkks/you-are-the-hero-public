using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// DRY RUN: report large assets that the build does NOT depend on (safe-ish to
// delete to speed up a cloud build). Excludes scripts, Resources/, Editor/.
public static class AnalyzeUnused
{
    public static string Execute()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;

        // everything the build needs (all enabled build scenes + their deps)
        var keep = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (!s.enabled) continue;
            keep.Add(s.path);
            foreach (var d in AssetDatabase.GetDependencies(s.path, true)) keep.Add(d);
        }

        var rows = new List<(string path, long bytes)>();
        long unusedTotal = 0;
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (!p.StartsWith("Assets/")) continue;
            if (keep.Contains(p)) continue;
            if (AssetDatabase.IsValidFolder(p)) continue;
            if (p.EndsWith(".cs") || p.EndsWith(".shader") || p.EndsWith(".asmdef") || p.EndsWith(".asmref")) continue;
            if (p.Contains("/Resources/") || p.Contains("/Editor/")) continue;
            string abs = Path.Combine(root, p);
            if (!File.Exists(abs)) continue;
            long len = new FileInfo(abs).Length;
            unusedTotal += len;
            if (len > 1024 * 1024) rows.Add((p, len));
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Build keeps " + keep.Count + " assets.");
        sb.AppendLine("Unused (non-build) assets >1MB: " + rows.Count + "; total unused footprint ~"
            + (unusedTotal / 1024.0 / 1024.0).ToString("0") + " MB.");
        sb.AppendLine("--- top unused assets ---");
        foreach (var r in rows.OrderByDescending(x => x.bytes).Take(45))
            sb.AppendLine((r.bytes / 1024.0 / 1024.0).ToString("0.0") + " MB  " + r.path);
        return sb.ToString();
    }
}
