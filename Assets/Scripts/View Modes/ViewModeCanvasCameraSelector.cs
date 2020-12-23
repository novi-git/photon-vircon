using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class ViewModeCanvasCameraSelector : MonoBehaviour
    {
        private void OnEnable()
        {
            ViewModeManager.Instance.ViewModeSelectedCallback += OnViewModeChanged;
            OnViewModeChanged( ViewModeManager.Instance.CurrentViewMode );
        }

        void OnViewModeChanged( ViewModeBase viewMode )
        {
            if( viewMode == null || viewMode.MainCamera == null )
            {
                return;
            }
            GetComponent<Canvas>().worldCamera = viewMode.MainCamera;
        }
    }
}
