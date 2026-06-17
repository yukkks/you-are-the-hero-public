using UnityEngine;

public class DoorVisualState : MonoBehaviour
{
    public int doorNumber = 1;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateDoorVisual();
    }

    void UpdateDoorVisual()
    {
        if (rend == null) return;

        bool completed = false;

        if (doorNumber == 1) completed = GameProgress.Instance.trial01Completed;
        if (doorNumber == 2) completed = GameProgress.Instance.trial02Completed;
        if (doorNumber == 3) completed = GameProgress.Instance.trial03Completed;

        if (completed)
        {
            rend.material.color = Color.green;
        }
    }
}