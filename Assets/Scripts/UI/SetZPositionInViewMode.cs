using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SetZPositionInViewMode : MonoBehaviour
    {
        public ViewModeManager.ViewMode ViewMode;
        public float ZPosition;
        public RectTransform TargetTransform;

        private void OnEnable()
        {
            if( ViewModeManager.SelectedViewMode == ViewMode )
            {
                TargetTransform.anchoredPosition3D = new Vector3( TargetTransform.anchoredPosition3D.x, TargetTransform.anchoredPosition3D.y, ZPosition );
            }
        }
    }
}