using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace C_Scripts.UI_Behaviours
{
    /// <summary>
    /// Exits current scene and loads the start menu scene.
    /// Placed on the "X" button, triggered by its UnityEvent.
    /// </summary>
    public class ExitToStartMenu : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 1.0f;
        [SerializeField] private string startMenuScene;
        [SerializeField] private Animator animator;

        public void GoToStartMenu()
        {
            StartCoroutine(FadeAndLoadRoutine());
        }

        private IEnumerator FadeAndLoadRoutine()
        {
            
            animator.SetBool("shouldFade", false);
            yield return new WaitForSeconds(fadeTime);
            
            // load start menu scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(startMenuScene);
            
            // launch next scene when loading is done
            while (!asyncLoad.isDone) yield return null;
        }
    }
}
