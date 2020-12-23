using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Voice.Unity;
namespace SpaceHub.Conference
{
    public class AudioSettingsManager : MonoBehaviour
    {
        Recorder m_Recorder { get { return ConferenceVoiceConnection.Instance?.PrimaryRecorder; } }

        private void Start()
        {
            float outputVolume = PlayerPrefs.GetFloat( "OutputVolume", 1f );
            SetOutputVolume( 0.5f );
#if !UNITY_WEBGL
            if( m_Recorder != null )
            {
                var amplifier = m_Recorder.GetComponent<Photon.Voice.Unity.UtilityScripts.MicAmplifier>();
                if( amplifier != null )
                {
                    float inputVolume = PlayerPrefs.GetFloat( "InputVolume", 0.8f );
                    amplifier.AmplificationFactor = inputVolume;
                }

                Recorder.PhotonMicrophoneEnumerator.Refresh();
                Application.RequestUserAuthorization( UserAuthorization.Microphone );

                try
                {
                    m_Recorder.UnityMicrophoneDevice = PlayerPrefs.GetString( "UnityMic" );         // set UNITY mic by string
                }
                catch( System.Exception e )
                {
                    Debug.LogException( e );
                }
                try
                {
                    m_Recorder.PhotonMicrophoneDeviceId = PlayerPrefs.GetInt( "PhotonMic", -1 );    // set PHOTON mic by ID. internally makes sure the microphone can be used or uses default (-1).
                }
                catch( System.Exception e )
                {
                    Debug.LogException( e );
                }
            }
#endif
        }

        public static void SetOutputVolume( float linearValue )
        {
            AudioListener.volume = linearValue;//Mathf.Log10( linearValue ) * 20;
        }
    }
}
