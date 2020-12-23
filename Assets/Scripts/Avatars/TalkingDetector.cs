using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;

namespace SpaceHub.Conference
{
    public class TalkingDetector : MonoBehaviour
    {
        AvatarCustomization m_Customizer;
        public float TalkThreshold = 0.005f;

        private void Awake()
        {
            m_Customizer = GetComponentInParent<AvatarCustomization>();
            
        }

        private void OnAudioFilterRead( float[] data, int channels )
        {
            float acc = 0f;
            foreach( var sample in data )
            {
                acc += Mathf.Abs( sample );
            }
            acc /= data.Length / channels;
            m_Customizer.IsTalking = acc > TalkThreshold;
        }
    }

}