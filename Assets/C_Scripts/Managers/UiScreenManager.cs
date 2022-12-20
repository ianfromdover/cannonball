using System.Collections.Generic;
using UnityEngine;

namespace C_Scripts.Managers
{
    /// <summary>
    /// Enables and disables groups of GameObjects of UI elements in the Game Scene.
    /// Methods called by UnityEvents on Buttons.
    /// </summary>
    public class UiScreenManager : MonoBehaviour
    {
        [SerializeField] private int currPage = 0;
        [SerializeField] private GameObject endModal;
        [SerializeField] private List<GameObject> screens = new List<GameObject>();
    
        void Start() { Enable(currPage); }
        private void Enable(int page) { screens[page].SetActive(true); }
        private void Disable(int page) { screens[page].SetActive(false); }

        private void EnableNextPage()
        {
            bool outOfBounds = (currPage < 0) || (currPage >= screens.Count);
            if (!outOfBounds) Enable(currPage);
        }
    
        // enable the next group of tutorial gameobjects
        public void NextGroup()
        {
            Disable(currPage);
            currPage++;
            EnableNextPage();
        }
    
        // enable the previous group of tutorial gameobjects
        public void PrevGroup()
        {
            Disable(currPage);
            currPage--;
            EnableNextPage();
        }
        
        // enable the previous group of tutorial gameobjects
        public void GoToGroup(int page)
        {
            Disable(currPage);
            currPage = page;
            EnableNextPage();
        }

        // enable and disable the pop-up when the timer runs out.
        public void EnableEndModal() { endModal.SetActive(true); }
        public void DisableEndModal() { endModal.SetActive(false); }
    }
}
