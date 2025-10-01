using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] string playSceneName = "GameScene";
    [SerializeField] string optionsSceneName = "OptionsScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(playSceneName);
    }

    public void OpenOptions()
    {
        SceneManager.LoadScene(optionsSceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
