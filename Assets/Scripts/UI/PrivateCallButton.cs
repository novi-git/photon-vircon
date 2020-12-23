using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class PrivateCallButton : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        Player m_Player;

        ConferencePrivateCalls PrivateCalls
        {
            get { return PlayerLocal.Instance.PrivateCalls; }
        }

        public void Setup( Player player )
        {
            m_Player = player;

            string text = "";
            if( m_Player.CustomProperties.ContainsKey( ConferenceCustomProperties.NickNamePropertyName ) )
            {
                text += (string)m_Player.CustomProperties[ ConferenceCustomProperties.NickNamePropertyName ] + "\n";
            }
            if( m_Player.CustomProperties.ContainsKey( ConferenceCustomProperties.CompanyNamePropertyName ) )
            {
                text += (string)m_Player.CustomProperties[ ConferenceCustomProperties.CompanyNamePropertyName ];
            }
            Text.text = text;
        }

        public void OnCancelOutgoingRequest()
        {
            PrivateCalls.CancelCallRequestTo( m_Player );
        }
        public void OnDeclineIncommingRequest()
        {
            PrivateCalls.DeclineCallFrom( m_Player );
        }
        public void OnAcceptIncommingRequest()
        {
            PrivateCalls.AcceptCallFrom( m_Player );
        }
    }
}
