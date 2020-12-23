using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace SpaceHub.Groups
{
    public class ChannelListButton : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public TextMeshProUGUI UnreadText;
        public GameObject NewMessagesObject;
        public GameObject PhoneImageActive;
        public GameObject PhoneImageJoined;

        public GameObject[] PrivateIcon;
        public GameObject[] PublicIcon;

        UnityAction<ChannelData> m_Callback;
        Button m_Button;
        int m_UnreadMessages = 0;

        ChannelData m_Data = new ChannelData();

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            SetSelected( "-1" );
            SetVoiceIcon( false, false );
            UpdateUnreadMessages();
        }

        public void Initialize( ChannelData channelData, UnityAction<ChannelData> onClickAction )
        {
            m_Button.onClick.AddListener( OnClick );

            m_Data = channelData;

            foreach( var item in PrivateIcon )
            {
                item?.SetActive( channelData.IsPrivate );
            }
            foreach( var item in PublicIcon )
            {
                item?.SetActive( !channelData.IsPrivate );
            }


            if( Label != null )
            {
                Label.text = channelData.Name;
            }
            m_Callback += onClickAction;


        }

        public void SetSelected( string channelName )
        {
            bool selected = channelName == m_Data.Name;
            m_Button.interactable = !selected;

            if( selected )
            {
                m_UnreadMessages = 0;
                UpdateUnreadMessages();
            }
        }

        public void SetVoiceIcon( bool active, bool joined )
        {
            if( PhoneImageActive )
            {
                PhoneImageActive.SetActive( active && !joined );
            }
            if( PhoneImageJoined )
            {
                PhoneImageJoined.SetActive( joined );
            }
        }

        public void IncreaseUnreadMessages()
        {
            ++m_UnreadMessages;
            UpdateUnreadMessages();
        }

        void OnClick()
        {
            m_Callback?.Invoke( m_Data );
        }

        void UpdateUnreadMessages()
        {
            NewMessagesObject.SetActive( m_UnreadMessages > 0 );

            if( m_UnreadMessages >= 100 )
            {
                UnreadText.text = "99+";
            }
            else
            {
                UnreadText.text = m_UnreadMessages.ToString();
            }
        }
    }
}
