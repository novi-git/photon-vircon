using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class CustomizationMenuLogin : MonoBehaviour
    {
        public GameObject BasicPanel;
        public GameObject OculusPanel;
        public GameObject LoginPanel;

        private void Start()
        {
            BasicPanel.SetActive( false );
            OculusPanel.SetActive( false );
            LoginPanel.SetActive( false );

//#if UNITY_ANDROID
        //    OculusPanel.SetActive( true );
//#else
            LoginPanel.SetActive( true );
//#endif
        }
    }
}