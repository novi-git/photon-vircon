using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class DisplayConnectivity : MonoBehaviour
    {
        public GameObject PhotonImage;
        public GameObject VoiceImage;

        void Update()
        {
            bool showPhotonError = PlayerLocal.Instance == null
                || PlayerLocal.Instance.Connector == null
                || PlayerLocal.Instance.Connector.Network == null
                || PlayerLocal.Instance.Connector.Network.Client == null
                || PlayerLocal.Instance.Connector.Network.Client.IsConnected == false;

            bool showVoiceError = ConferenceVoiceConnection.Instance == null
                || ConferenceVoiceConnection.Instance.Client == null
                || ConferenceVoiceConnection.Instance.Client.IsConnected == false;

            if( PhotonImage != null && showPhotonError != PhotonImage.activeSelf )
            {
                PhotonImage.SetActive( showPhotonError );
            }

            if( VoiceImage != null && showVoiceError != VoiceImage.activeSelf )
            {
                VoiceImage.SetActive( showVoiceError );
            }
        }
    }
}