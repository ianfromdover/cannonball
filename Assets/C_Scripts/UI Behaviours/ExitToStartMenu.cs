using UnityEngine;
using UnityEngine.SceneManagement;

namespace C_Scripts.UI_Behaviours
{
    /// <summary>
    /// Exits current scene and loads the start menu scene.
    /// Placed on the "X" button, triggered by its unityEvent.
    /// </summary>
    public class ExitToStartMenu : MonoBehaviour
    {
        [SerializeField] private string startMenuScene;
        public void GoToStartMenu() { SceneManager.LoadScene(startMenuScene); }
    }
}
