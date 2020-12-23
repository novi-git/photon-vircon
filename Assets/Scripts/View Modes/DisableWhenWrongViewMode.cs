using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceHub.Conference
{

    public class DisableWhenWrongViewMode : MonoBehaviour
    {
        public List<ViewModeManager.ViewMode> ValidViewModes;
        private void Awake()
        {
            if( ValidViewModes.Contains( ViewModeManager.Instance.CurrentViewMode.GetViewMode() ) == false )
            {
                gameObject.SetActive( false );
            }
        }
    }
}
