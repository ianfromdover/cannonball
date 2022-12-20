using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace C_Scripts.Managers
{
    public class StartMenuManager : MonoBehaviour
    {
        public EventChannelScore pointScored;
        public string nextScene;

        void Start() { pointScored.OnChange += StartGame; }
        void OnDestroy() { pointScored.OnChange -= StartGame; }
        public void StartGame(int unusedPoints) { StartCoroutine(StartGameRoutine()); }

        public IEnumerator StartGameRoutine()
        {
            // fade scene out
            // TODO: create fade out
            yield return new WaitForSeconds(2);
        
            // load next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
            
            // launch next scene when loading is done
            while (!asyncLoad.isDone) yield return null;
        }
    }
}
