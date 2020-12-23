using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Groups
{
    public class MinimizeElement : MonoBehaviour
    {
        public bool StartVisible = true;

        public GameObject[] ShowWhenVisible;
        public GameObject[] HideWhenVisible;
        bool m_Visible;

        private void Start()
        {
            SetVisible( StartVisible );
        }

        public void SetVisible( bool visible )
        {
            m_Visible = visible;
            UpdateState();
        }

        public void Toggle()
        {
            SetVisible( !m_Visible );
        }

        public void SetEnabled( bool enabled )
        {
            SetEnabledAndVisible( enabled, m_Visible );
        }

        public void SetEnabledAndVisible( bool enabled, bool visible )
        {
            m_Visible = visible;
            UpdateState();
            gameObject.SetActive( enabled );
        }

        void UpdateState()
        {
            foreach( var go in ShowWhenVisible )
            {
                go.SetActive( m_Visible );
            }

            foreach( var go in HideWhenVisible )
            {
                go.SetActive( !m_Visible );
            }
        }
    }
}