using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class ShowAndHideWhenInZone : MonoBehaviour
    {
        public GameObject[] ShowObjects;
        public GameObject[] HideObject;
        public List<ViewModeManager.ViewMode> Modes;

        public void SetVisible( bool value )
        {
            if( Modes.Contains( ViewModeManager.Instance.CurrentViewMode.GetViewMode() ) == false )
            {
                return;
            }

            foreach( var obj in ShowObjects )
            {
                obj.SetActive( value );
            }
            foreach( var obj in HideObject )
            {
                obj.SetActive( !value );
            }
        }

        private void OnTriggerEnter( Collider other )
        {
            SetVisible( true );
        }

        private void OnTriggerExit( Collider other )
        {
            SetVisible( false );
        }
    }
}
