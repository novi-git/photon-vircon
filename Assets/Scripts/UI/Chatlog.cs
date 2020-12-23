using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

namespace SpaceHub.Conference
{

    public class Chatlog : PopupBase
    {
        Groups.ChatConnector m_Chat { get { return PlayerLocal.Instance.ChatClient; } }

        public TextMeshProUGUI Text;
        string m_CurrentChannel;
        string[] m_Buffer = new string[ 30 ];
        int m_BufferIndex;

        StringBuilder m_Sb = new StringBuilder();

        private void Start()
        {
            DoClose();
        }

        public void JoinAndShowChannel( string channelName )
        {
            LeaveCurrentChannel();

            Debug.Log( "Chatlog: Join and Show channel " + channelName );
            m_CurrentChannel = channelName;
            m_Chat.RegisterCallback( channelName, OnChatMessage );

            Text.text = "";

            ClearBuffer();

            DoOpen();
        }

        void OnChatMessage( string sender, object data )
        {
            AddToBuffer( (string)data );
            Text.text = GetCompiledText();
        }

        void AddToBuffer( string message )
        {
            m_BufferIndex = ( m_BufferIndex + 1 ) % m_Buffer.Length;
            m_Buffer[ m_BufferIndex ] = message;
        }

        string GetCompiledText()
        {
            m_Sb.Clear();
            for( int i = 0; i < m_Buffer.Length; ++i )
            {
                int index = i + m_BufferIndex + 1;
                index = index % m_Buffer.Length;

                m_Sb.AppendLine( m_Buffer[ index ] );
            }
            return m_Sb.ToString();
        }

        void ClearBuffer()
        {
            for( int i = 0; i < m_Buffer.Length; ++i )
            {
                m_Buffer[ i ] = "";
            }
            m_BufferIndex = 0;
        }

        void LeaveCurrentChannel()
        {
            if( string.IsNullOrEmpty( m_CurrentChannel ) == false )
            {
                Debug.Log( "Chatlog: leaving channel " + m_CurrentChannel );
                m_Chat.UnregisterCallback( m_CurrentChannel, OnChatMessage );
            }
            m_CurrentChannel = "";
        }

        public void LeaveAndHideChannel()
        {
            LeaveCurrentChannel();
            DoClose();
        }
    }
}
