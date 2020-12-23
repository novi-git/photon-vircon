using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public abstract class ViewModeBase : MonoBehaviour
    {
        public Camera MainCamera;
        public XRSystem XRSystem;
        public GameObject[] EnableObjectsOnLoad;

        public virtual void OnViewModeSelected()
        {
            foreach( var go in EnableObjectsOnLoad )
            {
                go.SetActive( true );
            }
        }

        public virtual void OnViewModeDeselected()
        {
            foreach( var go in EnableObjectsOnLoad )
            {
                go.SetActive( false );
            }
        }

        public virtual bool SerializeXRSystem()
        {
            return false;
        }

        public abstract ViewModeManager.ViewMode GetViewMode();
    }
}