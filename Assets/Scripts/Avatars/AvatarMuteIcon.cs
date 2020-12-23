using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class AvatarMuteIcon : MonoBehaviour
    {
        public GameObject IconParent;
        int m_ShowFrameCount;

        public void Show()
        {
            if( IconParent != null && IsVisible() == false )
            {
                IconParent.SetActive( true );
            }

            m_ShowFrameCount = Time.frameCount;
        }

        public bool IsVisible()
        {
            return IconParent != null && IconParent.activeSelf;
        }

        void LateUpdate()
        {
            if( IconParent != null && IsVisible() == true && m_ShowFrameCount != Time.frameCount )
            {
                IconParent.SetActive( false );
            }
        }
    }
}