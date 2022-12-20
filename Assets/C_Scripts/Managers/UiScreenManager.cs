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
        [SerializeField] private List<GameObject> screens = new List<GameObject>();
        private int _currPage = 0;
    
        void Start() { Enable(_currPage); }
        private void Enable(int page) { screens[page].SetActive(true); }
        private void Disable(int page) { screens[page].SetActive(false); }

        private void EnableNextPage()
        {
            bool outOfBounds = (_currPage < 0) || (_currPage >= screens.Count);
            if (!outOfBounds) Enable(_currPage);
        }
    
        // enable the next group of tutorial gameobjects
        public void NextGroup()
        {
            Disable(_currPage);
            _currPage++;
            EnableNextPage();
        }
    
        // enable the previous group of tutorial gameobjects
        public void PrevGroup()
        {
            Disable(_currPage);
            _currPage--;
            EnableNextPage();
        }
        
        // enable the previous group of tutorial gameobjects
        public void GoToGroup(int page)
        {
            Disable(_currPage);
            _currPage = page;
            EnableNextPage();
        }
    }
}
