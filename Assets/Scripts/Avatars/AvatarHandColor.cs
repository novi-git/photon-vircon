using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class AvatarHandColor : MonoBehaviour
    {
        public Renderer HandRenderer;
        AvatarCustomization m_AvatarCustomization;
        void Start()
        {
            m_AvatarCustomization = GetComponentInParent<AvatarCustomization>();
            if ( m_AvatarCustomization != null)
            {
                m_AvatarCustomization.AddSkinRenderer( HandRenderer );
            }

        }

        private void OnDestroy()
        {
            if ( m_AvatarCustomization!= null )
            {
                m_AvatarCustomization.RemoveSkinRenderer( HandRenderer );
            }
        }

    }
}
