using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class PrivateCallPopup : PopupBase
    {
        public static PrivateCallPopup Instance;

        public TextMeshProUGUI NameText;
        public TextMeshProUGUI CompanyText;

        public RectTransform CallInfoPanel;
        public RectTransform RequestCallInfoPanel;
        

        void Start()
        {
            Instance = this;

            Close();
            //PlayerLocal.Instance.OnTeleportCallback += Close;
        }

        Player m_LastCallRequest;

        public void StopCall()
        {
            PlayerLocal.Instance.PrivateCalls.StopCall();            
        }

        public void AcceptCall()
        {
            Close();
            PlayerLocal.Instance.PrivateCalls.AcceptCallFrom( m_LastCallRequest );
        }

        public void DeclineCall()
        {
            Close();
            PlayerLocal.Instance.PrivateCalls.DeclineCallFrom( m_LastCallRequest );
        }

        public void OpenPrivateCallWith( Player player )
        {
            NameText.text = player.GetNickNameProperty();
            CompanyText.text = player.GetCompanyNameProperty();

            CallInfoPanel.gameObject.SetActive( true );
            RequestCallInfoPanel.gameObject.SetActive( false );

            DoOpen();
        }

        public void OpenCallRequestFrom( Player player )
        {
            m_LastCallRequest = player;

            NameText.text = player.GetNickNameProperty();
            CompanyText.text = player.GetCompanyNameProperty();

            CallInfoPanel.gameObject.SetActive( false );
            RequestCallInfoPanel.gameObject.SetActive( true );

            DoOpen();
        }

        public void Close()
        {
            DoClose();
        }
    }
}