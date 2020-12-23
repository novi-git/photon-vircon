using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class FramerateGuard : MonoBehaviour
    {
        //Oculus Quest always has a target framerate of 72 and we should hit that with the default quality settings we setup
#if !UNITY_ANDROID
        public int TargetFrameRate = 30;

        float m_LastTimeQuestionWasAnswered;
        bool m_IsQuestionOpen;
        float m_TimeInSuboptimalFramerate = 0;

        void Update()
        {
            if( ConnectingOverlay.IsOpen == true || QualitySettings.GetQualityLevel() == 0 )
            {
                m_TimeInSuboptimalFramerate = 0;
                return;
            }

            float frameRate = ( 1f / Time.unscaledDeltaTime );

            if( frameRate < TargetFrameRate )
            {
                m_TimeInSuboptimalFramerate += Time.unscaledDeltaTime;
            }
            else
            {
                m_TimeInSuboptimalFramerate = Mathf.MoveTowards( m_TimeInSuboptimalFramerate, 0f, Time.unscaledDeltaTime * 0.5f );
            }

            if( m_TimeInSuboptimalFramerate > 5f 
                && m_IsQuestionOpen == false 
                && QualitySettings.GetQualityLevel() > 0 )
            {
                m_IsQuestionOpen = true;
                string lowerName = QualitySettings.names[ QualitySettings.GetQualityLevel() - 1 ];
                MessagePopup.ShowConfirm( $"Your SpaceHub experience doesn't run very smoothly. Do you want to lower the quality to '{lowerName}' in order to increase performance?", "Yes", "No", OnConfirm, OnCancel );
            }
        }

        void OnCancel()
        {
            m_IsQuestionOpen = false;
            m_TimeInSuboptimalFramerate = 0;
        }

        void OnConfirm()
        {
            m_IsQuestionOpen = false;
            m_TimeInSuboptimalFramerate = 0;
            QualitySettings.SetQualityLevel( QualitySettings.GetQualityLevel() - 1, true );
            MessagePopup.Show( $"Quality settings set to '{QualitySettings.names[ QualitySettings.GetQualityLevel() ]}'" );

            Application.targetFrameRate = 60;
        }
#endif
    }
}