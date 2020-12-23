using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{


    public class UiAudioLevelDisplay : MonoBehaviour
    {

        public int Steps;
        public RectTransform LevelTransform;
        public RectTransform PeakTransform;

        float m_RollbackVolume;
        float m_PeakVolume;

        public void SetVolume( float value )
        {
            m_PeakVolume = Mathf.Max( m_PeakVolume, value );
            m_RollbackVolume = Mathf.Max( m_RollbackVolume, value );
        }

        private void Update()
        {
            m_RollbackVolume = Mathf.MoveTowards( m_RollbackVolume, 0f, Time.deltaTime * 0.25f );
            ScaleBar( LevelTransform, m_RollbackVolume );

            m_PeakVolume = Mathf.MoveTowards( m_PeakVolume, 0f, Time.deltaTime * 2f );
            ScaleBar( PeakTransform, m_PeakVolume );
        }

        void ScaleBar( Transform trans, float value )
        {
            if( trans == null )
            {
                return;
            }

            value = ( 1f - value );
            value *= value * value * value;
            value = 1f - value;

            if( Steps > 0 )
            {
                value = Mathf.Floor( value * Steps + 0.5f ) / Steps;
            }

            var localScale = Vector3.one;
            localScale.x = value;
            trans.localScale = localScale;
        }


    }
}