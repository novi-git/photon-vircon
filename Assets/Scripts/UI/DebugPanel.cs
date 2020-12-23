using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceHub.Conference
{

    public class DebugPanel : MonoBehaviour
    {
        public static string DisconnectCauseRealtime = "";
        public static string DisconnectCauseVoice = "";


        public TextMeshProUGUI RealtimeConnectionText;
        public TextMeshProUGUI VoiceConnectionText;
        public TextMeshProUGUI LogText;

        public TextMeshProUGUI RecorderBitrateHeadline;
        public TextMeshProUGUI RecorderSampleRateHeadline;
        public TextMeshProUGUI SpeakerDelayHeadline;

        public Slider RecorderBitrateSlider;
        public Slider RecorderSampleRateSlider;
        public Slider SpeakerDelaySlider;

        string m_RecorderBitratePrefix = "Bitrate: ";
        string m_RecorderSampleRatePrefix = "SampleRate: ";
        string m_SpeakerDelayPrefix = "Playback Delay: ";

        public Button ShowWarningsAsPopupButton;

        private void Awake()
        {
            if( ShowWarningsAsPopupButton != null )
            {
                ShowWarningsAsPopupButton.onClick.AddListener( ToggleShowWarningsAsPopup );
            }
        }

        private void OnEnable()
        {
            StartCoroutine( OnEnableRoutine() );
            StartCoroutine( SetupSliders() );
        }

        IEnumerator OnEnableRoutine()
        {
            yield return null;

            if( LogManager.Instance == null )
            {
                yield break;
            }

            LogManager.Instance.LogChangedCallback += OnLogChanged;
            OnLogChanged();
        }

        private void OnDisable()
        {
            if( LogManager.Instance == null )
            {
                return;
            }
            LogManager.Instance.LogChangedCallback -= OnLogChanged;
        }

        void OnLogChanged()
        {
            LogText.text = LogManager.Instance.GetLastLogs();
        }

        IEnumerator SetupSliders()
        {
            RecorderBitrateSlider.interactable = false;
            RecorderSampleRateSlider.interactable = false;
            SpeakerDelaySlider.interactable = false;

            RecorderBitrateHeadline.text = m_RecorderBitratePrefix;
            RecorderSampleRateHeadline.text = m_RecorderSampleRatePrefix;
            SpeakerDelayHeadline.text = m_SpeakerDelayPrefix;

            while( PlayerLocal.Instance == null || PlayerLocal.Instance.VoiceConnection == null || PlayerLocal.Instance.VoiceConnection.PrimaryRecorder == null )
            {
                yield return null;
            }

            RecorderBitrateSlider.SetValueWithoutNotify( PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.Bitrate );
            RecorderSampleRateSlider.SetValueWithoutNotify( (int)PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.SamplingRate );

            RecorderBitrateSlider.interactable = true;
            RecorderSampleRateSlider.interactable = true;

            SpeakerDelaySlider.SetValueWithoutNotify( PlayerLocal.Instance.VoiceConnection.GlobalPlaybackDelay );

            SpeakerDelaySlider.interactable = true;
        }

        private void Update()
        {
            if( PlayerLocal.Instance == null ||
                PlayerLocal.Instance.Client == null )
            {
                return;
            }

            var realtimeClient = PlayerLocal.Instance.Client;
            RealtimeConnectionText.text = "Photon Realtime Status:\nState:" + realtimeClient.State;
            if( realtimeClient.State == Photon.Realtime.ClientState.Disconnected )
            {
                RealtimeConnectionText.text += "\nReason: " + DisconnectCauseRealtime;
            }

            if( PlayerLocal.Instance.VoiceConnection != null )
            {
                var voiceClient = PlayerLocal.Instance.VoiceConnection.Client;
                VoiceConnectionText.text = "Photon Realtime Status:\nState:" + voiceClient.State;
                if( voiceClient.State == Photon.Realtime.ClientState.Disconnected )
                {
                    VoiceConnectionText.text += "\nReason: " + DisconnectCauseRealtime;
                }
                VoiceConnectionText.text += "\nMic: " + PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.UnityMicrophoneDevice;

                if( PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.IsCurrentlyTransmitting )
                {
                    VoiceConnectionText.text += "\nLevel: " + PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.LevelMeter.CurrentPeakAmp.ToString( "0.00" );
                }
            }
        }

        public void OnBitrateValueChanged( float value )
        {
            int newValue = (int)( RecorderBitrateSlider.value / 1000 ) * 1000;
            RecorderBitrateHeadline.text = m_RecorderBitratePrefix + newValue;

            if( PlayerLocal.Instance != null &&
                PlayerLocal.Instance.VoiceConnection != null )
            {
                PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.Bitrate = newValue;
                PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.RestartRecording();
            }
        }

        public void OnSampleRateValueChanged( float value )
        {
            POpusCodec.Enums.SamplingRate newValue = POpusCodec.Enums.SamplingRate.Sampling08000;

            if( RecorderSampleRateSlider.value > 8000 )
            {
                newValue = POpusCodec.Enums.SamplingRate.Sampling12000;
            }
            if( RecorderSampleRateSlider.value > 12000 )
            {
                newValue = POpusCodec.Enums.SamplingRate.Sampling16000;
            }
            if( RecorderSampleRateSlider.value > 16000 )
            {
                newValue = POpusCodec.Enums.SamplingRate.Sampling24000;
            }
            if( RecorderSampleRateSlider.value > 24000 )
            {
                newValue = POpusCodec.Enums.SamplingRate.Sampling48000;
            }

            RecorderSampleRateSlider.SetValueWithoutNotify( (int)newValue );

            RecorderSampleRateHeadline.text = m_RecorderSampleRatePrefix + newValue;

            if( PlayerLocal.Instance != null &&
                PlayerLocal.Instance.VoiceConnection != null )
            {
                PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.SamplingRate = newValue;
                PlayerLocal.Instance.VoiceConnection.PrimaryRecorder.RestartRecording();
            }
        }

        public void OnPlaybackDelayValueChanged( float value )
        {
            int newValue = (int)SpeakerDelaySlider.value;
            SpeakerDelayHeadline.text = m_SpeakerDelayPrefix + newValue;

            PlayerLocal.Instance.VoiceConnection.GlobalPlaybackDelay = newValue;
        }

        public void ToggleShowWarningsAsPopup()
        {
            LogManager.ShowWarningPopup = !LogManager.ShowWarningPopup;

            string text = LogManager.ShowWarningPopup ? "[x]" : "[  ]";
            ShowWarningsAsPopupButton.GetComponentInChildren<TextMeshProUGUI>().text = text + " Show Warnings as Popups";
        }
    }

}
