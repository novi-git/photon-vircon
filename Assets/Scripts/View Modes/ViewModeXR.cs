using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace SpaceHub.Conference
{
    public class ViewModeXR : ViewModeBase
    {
        public override void OnViewModeSelected()
        {
            base.OnViewModeSelected();

            StartCoroutine( LoadXR() );            
        }

        private IEnumerator LoadXR()
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();

            XRManagerSettings manager = XRGeneralSettings.Instance.Manager;

            Debug.Log( "LoadXR! " + manager.activeLoader );

            if(manager.activeLoader == null  )
            {
                StartCoroutine( manager.InitializeLoader() );
            }

            while( manager.isInitializationComplete == false )
                yield return waitForEndOfFrame;

            yield return waitForEndOfFrame;

            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }

        public override ViewModeManager.ViewMode GetViewMode()
        {
            return ViewModeManager.ViewMode.XR;
        }

        public override bool SerializeXRSystem()
        {
            return true;
        }
    }
}