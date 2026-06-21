using UnityEditor;

// Remove the now-unused real-time finale scripts (the ending is an external video
// in the web wrapper; completion is signalled via GameCompleteSignal).
public static class DeleteFinaleScripts
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder("Deleted: ");
        if (AssetDatabase.DeleteAsset("Assets/Scripts/FinaleController.cs")) sb.Append("FinaleController.cs; ");
        if (AssetDatabase.DeleteAsset("Assets/Scripts/FinaleVideo.cs")) sb.Append("FinaleVideo.cs; ");
        AssetDatabase.Refresh();
        return sb.ToString();
    }
}
