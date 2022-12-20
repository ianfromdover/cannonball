using System.Collections;
using C_Scripts.Event_Channels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace C_Scripts.Managers
{
    public class StartMenuManager : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 3.0f;
        [SerializeField] private Animator animator;
        [SerializeField] private EventChannelScore pointScored;
        public string nextScene;

        void Start()
        {
            pointScored.OnChange += StartGame;
        }

        void OnDestroy()
        {
            pointScored.OnChange -= StartGame;
        }

        /// <summary>
        /// Launches the game when the target is hit.
        /// </summary>
        public void StartGame(int unusedPoints)
        {
            StartCoroutine(StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            // fade scene out
            animator.SetBool("shouldFade", true);
            yield return new WaitForSeconds(fadeTime);
        
            // load next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
            
            // launch next scene when loading is done
            while (!asyncLoad.isDone) yield return null;
        }
    }
}
