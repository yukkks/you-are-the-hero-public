using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalUnlock : MonoBehaviour
{
    private Renderer rend;
    private bool unlocked = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        CheckUnlockState();
    }

    void CheckUnlockState()
    {
        unlocked = GameProgress.Instance.trial01Completed &&
                   GameProgress.Instance.trial02Completed &&
                   GameProgress.Instance.trial03Completed;

        if (unlocked && rend != null)
        {
            rend.material.color = Color.yellow;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (unlocked && other.CompareTag("Player"))
        {
            SceneManager.LoadScene("FinalRestore");
        }
    }
}