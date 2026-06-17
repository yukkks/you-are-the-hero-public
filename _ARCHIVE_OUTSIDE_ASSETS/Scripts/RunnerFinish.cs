using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerFinish : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameProgress.Instance.trial01Completed = true;
            SceneManager.LoadScene("HubWorld");
        }
    }
}