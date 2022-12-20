using System.Collections.Generic;
using UnityEngine;

namespace C_Scripts.Managers
{
    /// <summary>
    /// Enables and disables groups of GameObjects of UI elements in the AR Cannon Scene.
    /// Methods called by UnityEvents on Buttons.
    /// </summary>
    public class UiScreenManager : MonoBehaviour
    {
        [SerializeField] private int currPage = 0;
        [SerializeField] private GameObject endModal;
        [SerializeField] private List<GameObject> screens = new List<GameObject>();

        void Start()
        {
            Enable(currPage);
        }

        /* --- helper functions --- */
        private void Enable(int page)
        {
            screens[page].SetActive(true);
        }

        private void Disable(int page)
        {
            screens[page].SetActive(false);
        }

        private void EnableNextPage()
        {
            bool outOfBounds = (currPage < 0) || (currPage >= screens.Count);
            if (!outOfBounds) Enable(currPage);
        }
        /* --- helper functions end --- */
    
        /// <summary>
        /// Enables the next group of UI Elements
        /// </summary>
        public void NextGroup()
        {
            Disable(currPage);
            currPage++;
            EnableNextPage();
        }
    
        /// <summary>
        /// Enables the previous group of UI Elements
        /// </summary>
        public void PrevGroup()
        {
            Disable(currPage);
            currPage--;
            EnableNextPage();
        }
        
        /// <summary>
        /// Enables a specific group of UI Elements
        /// </summary>
        public void GoToGroup(int page)
        {
            Disable(currPage);
            currPage = page;
            EnableNextPage();
        }

        /// <summary>
        /// Enables pop-up end modal when the timer runs out.
        /// </summary>
        public void EnableEndModal()
        {
            endModal.SetActive(true);
        }
    }
}
