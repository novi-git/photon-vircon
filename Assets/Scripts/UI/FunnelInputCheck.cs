using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SpaceHub.Conference
{
    public class FunnelInputCheck : MonoBehaviour
    {
        public UiAudioLevelDisplay AudioLevel;
        public TMP_Dropdown DeviceDropdown;
        public Graphic MicImageOn;
        public Graphic MicImageOff;

        public GameObject ConfirmationCheckmarkMic;
        public GameObject ConfirmationCheckmarkOutput;

        public GameObject[] HiddenWhenDisabled;

        UIFunnelScreen m_Screen;

        string m_CurrentDeviceName = null;
        bool m_MicConfirmed;
        bool m_OutputConfirmed;

        AudioClip m_Clip;
        public AudioSource Audio;
        public float TargetVolume = .6f;

        void Awake()
        {
            DeviceDropdown.onValueChanged.AddListener( StartRecording );


            m_CurrentDeviceName = PlayerPrefs.GetString( "UnityMic", null );
            m_Screen = GetComponentInParent<UIFunnelScreen>();
            m_Screen.ShowCallback += OnShow;
            m_Screen.HideCallback += OnHide;
        }

        IEnumerator FadeMusicRoutine()
        {
            Audio.volume = 0f;
            Audio.Play();

            float volume = 0f;
            while( true )
            {
                volume = Mathf.MoveTowards( volume, m_Screen.IsVisible ? 1f : 0f, Time.deltaTime );
                Audio.volume = volume * TargetVolume;
                yield return null;
            }
        }

        void OnHide()
        {
            StopRecording();
        }

        void OnShow()
        {
#if UNITY_WEBGL
            m_MicConfirmed = true;
#else
            UpdateMicrophones();
            SelectMicrophone( m_CurrentDeviceName );
#endif
        }

        private void OnEnable()
        {
            StartCoroutine( GetVolumeRoutine() );

            if( Audio != null )
            {
                StartCoroutine( FadeMusicRoutine() );
            }
        }

        public void DisableMic()
        {
            StopRecording();
            DeviceDropdown.SetValueWithoutNotify( 0 );
            m_CurrentDeviceName = "Disabled";
            PlayerPrefs.SetString( "UnityMic", "Disabled" );
            m_MicConfirmed = false;
            m_OutputConfirmed = false;

            SetMicActive( false );
        }

        public void ConfirmMic()
        {
            m_MicConfirmed = true;
            CheckNext();
        }
        public void ConfirmOutput()
        {
            m_OutputConfirmed = true;
            CheckNext();
        }
        
        public void CheckNext()
        {
            if ( m_OutputConfirmed && m_MicConfirmed )
            {
                //GetComponentInParent<UiFunnelManager>().Next();
                PlayerPrefs.SetString("EventId", "");
                PlayerPrefs.SetString("PhotonRegion", "jp");
                SceneManager.LoadScene("CustomizationRoom");
            }
        }

        void UpdateMicrophones()
        {
            DeviceDropdown.ClearOptions();

            var deviceList = new List<string>( new string[] { "Disabled" } );
#if !UNITY_WEBGL
            deviceList.AddRange( Microphone.devices );
#endif
            DeviceDropdown.AddOptions( deviceList );
        }

        void SelectMicrophone( string deviceName )
        {
            int index = DeviceDropdown.options.FindIndex( item => item.text == deviceName );
            DeviceDropdown.SetValueWithoutNotify( index );
            DeviceDropdown.onValueChanged?.Invoke( index );
        }

        void StartRecording( int index )
        {
            if ( index < 0 || index >= DeviceDropdown.options.Count )
            {
                index = DeviceDropdown.options.Count > 1 ? 1 : 0;
            }
            if( index == 0 )
            {
                DisableMic();
                return;
            }
            m_MicConfirmed = false;
            SetMicActive( true );
            StartRecording( DeviceDropdown.options[ index ].text );
        }

        void StartRecording( string deviceName )
        {
#if !UNITY_WEBGL
            if( Microphone.IsRecording( m_CurrentDeviceName ) )
            {
                StopRecording();
            }

            m_CurrentDeviceName = deviceName;
            PlayerPrefs.SetString( "UnityMic", deviceName );


            Debug.Log( "Start Recording: " + deviceName );
            if( deviceName == "Disabled" )
            {
                return;
            }

            Microphone.GetDeviceCaps( m_CurrentDeviceName, out var minFreq, out var maxFreq );
            m_Clip = Microphone.Start( m_CurrentDeviceName, true, 1, maxFreq );
#endif
        }

        void StopRecording()
        {
#if !UNITY_WEBGL
            Microphone.End( m_CurrentDeviceName );
            m_Clip = null;
#endif
        }

        void SetMicActive( bool value )
        {
            foreach( var go in HiddenWhenDisabled )
            {
                go.SetActive( value );
            }
        }

        void Update()
        {
            if( MicImageOn != null && MicImageOff != null )
            {
                bool isRecording = false;

#if !UNITY_WEBGL
                isRecording = Microphone.IsRecording( m_CurrentDeviceName );
#endif

                MicImageOn.enabled = isRecording;
                MicImageOff.enabled = !isRecording;
            }

#if !UNITY_WEBGL
            if( m_MicConfirmed != ConfirmationCheckmarkMic.activeSelf )
            {
                ConfirmationCheckmarkMic.SetActive( m_MicConfirmed );
            }
#endif
            if( m_OutputConfirmed != ConfirmationCheckmarkOutput.activeSelf )
            {
                ConfirmationCheckmarkOutput.SetActive( m_OutputConfirmed );
            }
        }

        public void SetSpeakerVolume( float value )
        {
            AudioListener.volume = value;
        }

        IEnumerator GetVolumeRoutine()
        {
#if UNITY_WEBGL
            AudioLevel.SetVolume( 0f );
            yield break;
#else
            var wait = new WaitForSeconds( 0.05f );

            int sampleCount = 128;
            float[] samples = new float[ sampleCount ];
            while( true )
            {
                yield return wait;
                if( m_Clip == null || m_Clip.length == 0 )
                {
                    AudioLevel.SetVolume( 0f );
                    continue;
                }

                int position = Microphone.GetPosition( m_CurrentDeviceName ) - sampleCount;
                if( position < 0 )
                {
                    position = m_Clip.samples + position;
                }

                if( m_Clip.GetData( samples, position ) )
                {
                    float volume = 0f;
                    float peak = 0f;
                    foreach( var sample in samples )
                    {
                        float absValue = Mathf.Abs( sample );
                        volume += absValue;
                        peak = Mathf.Max( peak, absValue );
                    }
                    volume /= sampleCount;
                    AudioLevel.SetVolume( volume );
                }
                else
                {
                    AudioLevel.SetVolume( 0f );
                    Debug.LogWarning( "Could not get samples from mic " + m_CurrentDeviceName );
                }
            }
#endif
        }
    }
}