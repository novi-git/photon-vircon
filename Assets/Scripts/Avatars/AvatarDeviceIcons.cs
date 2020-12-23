using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class AvatarDeviceIcons : MonoBehaviour
    {
        public enum DeviceType
        {
            XR,
            PC,
            WEB
        }

        public const string ClientDeviceTypePropertyName = "deviceType";


        public GameObject XRHeadset;
        public GameObject WebIcon;

        private void Start()
        {
            Apply( DeviceType.PC );
            var avatar = GetComponentInParent<Avatar>();
            if( avatar == null || avatar.CustomProperties == null )
            {
                return;
            }
            avatar.CustomProperties.RegisterPlayerPropertiesChangedCallback( OnPlayerPropertiesChanged );
        }

        private void OnDestroy()
        {
            var avatar = GetComponentInParent<Avatar>();
            if( avatar != null && avatar.CustomProperties != null )
            {
                avatar.CustomProperties.UnregisterPlayerPropertiesChangedCallback( OnPlayerPropertiesChanged );
            }
        }

        void OnPlayerPropertiesChanged( ExitGames.Client.Photon.Hashtable changedProps )
        {
            if( changedProps.ContainsKey( ClientDeviceTypePropertyName ) )
            {
                Apply( (DeviceType)changedProps[ ClientDeviceTypePropertyName ] );
            }
        }

        public void Apply( DeviceType device )
        {
            if( XRHeadset != null )
            {
                XRHeadset.SetActive( device == DeviceType.XR );
            }
            if( WebIcon != null )
            {
                WebIcon.SetActive( device == DeviceType.WEB );
            }
        }
    }
}
