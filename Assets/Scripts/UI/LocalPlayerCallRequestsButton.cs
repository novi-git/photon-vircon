using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace SpaceHub.Conference
{
    public class LocalPlayerCallRequestsButton : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        void UpdateText( ConferenceCustomProperties.CallStatusType? type = null )
        {
            if( PlayerLocal.Instance == null )
            {
                return;
            }

            if( type == null )
            {
                type = PlayerLocal.Instance.CustomProperties.CallStatus;
            }

            switch( type )
            {
            case ConferenceCustomProperties.CallStatusType.Available:
                Text.text = "Enabled";
                break;
            case ConferenceCustomProperties.CallStatusType.DoNotDisturb:
                Text.text = "Do not disturb";
                break;
            }
        }

        private void OnEnable()
        {
            UpdateText();
        }

        public void OnClick()
        {
            switch( PlayerLocal.Instance.CustomProperties.CallStatus )
            {
            case ConferenceCustomProperties.CallStatusType.Available:
                SetCallStatus( ConferenceCustomProperties.CallStatusType.DoNotDisturb );
                break;
            default:
                SetCallStatus( ConferenceCustomProperties.CallStatusType.Available );
                break;
            }
            
            EventSystem.current.SetSelectedGameObject( null );
        }

        void SetCallStatus( ConferenceCustomProperties.CallStatusType type )
        {
            PlayerLocal.Instance.CustomProperties.CallStatus = type;
            UpdateText( type );
        }
    }
}