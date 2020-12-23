using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceHub.Conference
{
    public class WatchUI : MonoBehaviour
    {
        public void OnClicked()
        {
            if( MainMenuPopup.Instance != null )
            {
                MainMenuPopup.Instance.Open();
            }

            if( EventSystem.current != null )
            {
                EventSystem.current.SetSelectedGameObject( null );
            }
        }
    }
}
