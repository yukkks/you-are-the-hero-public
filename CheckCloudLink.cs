using UnityEditor;

public static class CheckCloudLink
{
    public static string Execute()
    {
        string pid = CloudProjectSettings.projectId;
        return "linkedToUnityCloud=" + !string.IsNullOrEmpty(pid)
            + "; projectId='" + pid + "'"
            + "; user='" + CloudProjectSettings.userName + "'";
    }
}
