using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class XRSceneSettingsHandler : MonoBehaviour
    {
        public UnityEngine.XR.Interaction.Toolkit.TeleportationProvider TeleportationProvider;
        public UnityEngine.XR.Interaction.Toolkit.XRInteractionManager InteractionManager;

        private void Update()
        {
            if( ConferenceSceneSettings.Instance == null )
            {
                return;
            }

            SetObjectEnabled( TeleportationProvider, ConferenceSceneSettings.Instance.IsTeleportEnabled() );
        }


        void SetObjectEnabled( MonoBehaviour target, bool value )
        {
            if( target.enabled != value )
            {
                target.enabled = value;
            }
        }
    }
}
