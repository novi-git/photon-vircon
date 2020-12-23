using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace SpaceHub.Groups
{
    public class UserListButton : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        User m_User;
        Button m_Button;
        UnityAction<User> m_Callback;

        public Graphic VoiceIcon;

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener( OnClick );
            VoiceIcon.enabled = false;
        }

        public void SetUser( User user, UnityAction<User> callback )
        {
            m_User = user;
            Label.text = m_User.Nickname;
            m_Callback += callback;
        }

        public void SetVoiceIconVisible( bool value )
        {
            if( VoiceIcon != null )
            {
                VoiceIcon.enabled = value;
            }
        }

        void OnClick()
        {
            m_Callback?.Invoke( m_User );
        }
    }
}
