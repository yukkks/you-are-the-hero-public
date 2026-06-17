using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerObstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Trial01_Runner");
        }
    }
}