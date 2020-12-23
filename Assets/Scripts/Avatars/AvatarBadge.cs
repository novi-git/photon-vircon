using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class AvatarBadge : MonoBehaviour
    {
        public string NickName;
        public string CompanyName;

        public TextMeshPro NameText;
        public TextMeshPro CompanyText;

        private void Start()
        {
            ApplyNames();
            var avatar = GetComponentInParent<Avatar>();
            if( avatar == null || avatar.CustomProperties == null )
            {
                return;
            }

            avatar.CustomProperties.RegisterPlayerPropertiesChangedCallback( OnPlayerPropertiesChanged );

            //avatar.CustomProperties.PlayerPropertiesChangedCallback += OnPlayerPropertiesChanged;
            //OnPlayerPropertiesChanged( avatar.CustomProperties.Player.CustomProperties );
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
            if( changedProps.ContainsKey( ConferenceCustomProperties.NickNamePropertyName ) )
            {
                NickName = (string)changedProps[ ConferenceCustomProperties.NickNamePropertyName ];
            }
            if( changedProps.ContainsKey( ConferenceCustomProperties.CompanyNamePropertyName ) )
            {
                CompanyName = (string)changedProps[ ConferenceCustomProperties.CompanyNamePropertyName ];
            }

            ApplyNames();
        }

        public void ApplyNames()
        {
            NameText.text = NickName;
            CompanyText.text = CompanyName;
        }
    }
}
