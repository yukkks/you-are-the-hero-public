using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterGateLoader : MonoBehaviour
{
    public string sceneToLoad;

    void OnMouseDown()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}