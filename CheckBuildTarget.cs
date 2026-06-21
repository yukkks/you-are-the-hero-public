using UnityEditor;
using UnityEngine;

public static class CheckBuildTarget
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("activeBuildTarget=" + EditorUserBuildSettings.activeBuildTarget + "; ");
        bool webgl = false;
        try { webgl = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL); } catch { }
        sb.Append("webglModuleInstalled=" + webgl + "; scenes=");
        foreach (var s in EditorBuildSettings.scenes)
            sb.Append(s.path + (s.enabled ? "(on) " : "(off) "));
        return sb.ToString();
    }
}
