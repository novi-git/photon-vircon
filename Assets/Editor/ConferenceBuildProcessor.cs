using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine.SceneManagement;

namespace SpaceHub.Conference
{
    public class ConferenceBuildProcessor : IProcessSceneWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnProcessScene( Scene scene, BuildReport report )
        {
            var gos = scene.GetRootGameObjects();

            foreach( var go in gos )
            {
                var settings = go.GetComponent<ConferenceServerSettings>();

                if( settings != null )
                {
                    settings.EnableBuildMode();
                }
            }
        }
    }
}
