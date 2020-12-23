using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;

namespace SpaceHub.Conference
{

    public class TalkingDetectorLocal : MonoBehaviour
    {
        public float Threshold = 0.15f;
        AvatarCustomization m_Customizer;
        Recorder m_Recorder;
        private IEnumerator Start()
        {
            while( ViewModeManager.Instance == null || ViewModeManager.Instance.CurrentViewMode == null )
            {
                yield return null;
            }
            m_Customizer = ViewModeManager.Instance.CurrentViewMode.XRSystem.GetComponentInChildren<AvatarCustomization>();
            m_Recorder = GetComponent<Recorder>();
        }


        void Update()
        {
            if( m_Recorder == null || m_Customizer == null )
            {
                return;
            }

            m_Customizer.IsTalking = m_Recorder.IsCurrentlyTransmitting && m_Recorder.LevelMeter.CurrentPeakAmp > Threshold;
        }
    }
}
