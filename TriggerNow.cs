using UnityEngine;

// Test helper: force the finale to run (skips having to greet/view everything).
public static class TriggerNow
{
    public static string Execute()
    {
        var fc = GameObject.Find("GameManager").GetComponent<FinaleController>();
        if (fc == null) return "FinaleController not found";
        fc.TriggerFinale();
        return "Finale triggered.";
    }
}
