using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Voice.Unity;

namespace SpaceHub.Conference
{
    public class AudioSettingsMenu : MonoBehaviour
    {
        public Slider OutputVolume;
        public Slider InputVolume;

        public RectTransform MicrophoneParent;
        public GameObject ButtonPrefab;
        public GameObject LabelPrefab;

        Recorder m_Recorder { get { return ConferenceVoiceConnection.Instance?.PrimaryRecorder; } }

        public AudioSource SoundcheckAudio;

        private void OnEnable()
        {
            if( m_Recorder == null )
            {
                return;
            }

            InputVolume.onValueChanged.RemoveListener( OnInputVolumeChanged );
            OutputVolume.onValueChanged.RemoveListener( OnOutputVolumeChanged );

            InputVolume.value = PlayerPrefs.GetFloat( "InputVolume", 0.8f );
            OutputVolume.value = PlayerPrefs.GetFloat( "OutputVolume", 1f );

            InputVolume.onValueChanged.AddListener( OnInputVolumeChanged );

            OutputVolume.onValueChanged.AddListener( OnOutputVolumeChanged );

            CreateMicrophoneButtons();
        }


        public void CreateMicrophoneButtons()
        {
            Recorder.PhotonMicrophoneEnumerator.Refresh();

            Helpers.DestroyAllChildren( MicrophoneParent );
            List<string> options = new List<string>();
            int selectedIndex = -1;

#if !UNITY_WEBGL
            if( m_Recorder.MicrophoneType == Recorder.MicType.Unity )
            {
                options = new List<string>( Microphone.devices );
                selectedIndex = options.FindIndex( item => Recorder.CompareUnityMicNames( item, m_Recorder.UnityMicrophoneDevice ) );
            }
            else
            {
                var devices = Recorder.PhotonMicrophoneEnumerator;
                for( int i = 0; i < devices.Count; ++i )
                {
                    options.Add( devices.NameAtIndex( i ) );
                    if( devices.IDAtIndex( i ) == m_Recorder.PhotonMicrophoneDeviceId )
                    {
                        selectedIndex = i;
                    }
                }
            }
#endif

            for( int i = 0; i < options.Count; ++i )
            {
                GameObject go = null;
                if( i == selectedIndex )
                {
                    go = Instantiate( LabelPrefab, MicrophoneParent );
                }
                else
                {
                    int index = i;
                    go = Instantiate( ButtonPrefab, MicrophoneParent );
                    var button = go.GetComponent<Button>();

                    if( button != null )
                    {
                        button.onClick.AddListener( delegate ()
                        {
                            OnInputDeviceChanged( index );
                        } );
                    }
                }

                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                label.text = options[ i ];
            }
        }

        void OnInputVolumeChanged( float value )
        {
            PlayerPrefs.SetFloat( "InputVolume", value );

            var amplifier = m_Recorder.GetComponent<Photon.Voice.Unity.UtilityScripts.MicAmplifier>();

            if( amplifier != null )
            {
                amplifier.AmplificationFactor = value;
            }
        }

        void OnOutputVolumeChanged( float value )
        {
            PlayerPrefs.SetFloat( "OutputVolume", value );
            AudioSettingsManager.SetOutputVolume( value );
        }

        public void OnOutputVolumeDragEnd()
        {
            SoundcheckAudio.Play();
        }


        void OnInputDeviceChanged( int value )
        {
#if !UNITY_WEBGL
            if( m_Recorder.MicrophoneType == Recorder.MicType.Unity )
            {
                try
                {
                    m_Recorder.UnityMicrophoneDevice = Microphone.devices[ value ];
                    PlayerPrefs.SetString( "UnityMic", m_Recorder.UnityMicrophoneDevice );
                }
                catch( System.Exception e )
                {
                    Debug.LogException( e );
                }
            }
            else
            {
                try
                {
                    m_Recorder.PhotonMicrophoneDeviceId = Recorder.PhotonMicrophoneEnumerator.IDAtIndex( value );
                    PlayerPrefs.SetInt( "PhotonMic", m_Recorder.PhotonMicrophoneDeviceId );
                }
                catch( System.Exception e )
                {
                    Debug.LogException( e );
                }
            }

            if( m_Recorder.RequiresRestart )
            {
                m_Recorder.RestartRecording();
            }

            CreateMicrophoneButtons();
#endif
        }
    }
}