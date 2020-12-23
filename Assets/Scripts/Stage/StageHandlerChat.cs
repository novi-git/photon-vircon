using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class StageHandlerChat : StageHandlerBase
    {
        StageChatConnection m_Connection;

        float m_NextPing;
        float m_LastReceivedPing;

        public float PingDelay = 1f;
        public float PingTimeoutDelay = 3f;


        private void Awake()
        {
            m_Connection = GetComponent<StageChatConnection>();
            m_Connection.OnGetMessagesCallback += OnGetMessages;
        }

        private IEnumerator Start()
        {
            while( IsConnected() == false )
            {
                yield return null;
            }

            PlayerLocal.Instance.VoiceConnection.JoinRoom( "VoiceStageChat" );

            RoomJoinedCallback?.Invoke();
        }
        private void OnDestroy()
        {
            PlayerLocal.Instance.VoiceConnection.LeaveRoom();
        }

        public override bool IsConnected()
        {
            return m_Connection != null && m_Connection.IsConnected();
        }

        public override bool IsInRoom()
        {
            return IsConnected();
        }

        public override bool IsSpeakerPresent()
        {
            return Time.realtimeSinceStartup < ( m_LastReceivedPing + PingTimeoutDelay );
        }

        public void OnGetMessages( string channelName, string[] senders, object[] messages )
        {
            switch( channelName )
            {
            case StageChatConnection.SpeakerXRChannelName:
                SpeakerXRCallback?.Invoke( (ExitGames.Client.Photon.Hashtable)messages[ messages.Length - 1 ] );
                break;
            case StageChatConnection.SpeakerXRCustomizationChannelName:
                SpeakerCustomizationCallback?.Invoke( (ExitGames.Client.Photon.Hashtable)messages[ messages.Length - 1 ] );
                break;
            case StageChatConnection.SpeakerPingChannelName:
                OnGetSpeakerPing();
                break;
            case StageChatConnection.PresentationChannelName:
                PresentationCallback?.Invoke( (int)messages[ messages.Length - 1 ] );
                break;
            case StageChatConnection.EmoteChannelName:
                for( int i = 0; i < senders.Length; ++i )
                {
                    if( senders[ i ] == StageChatConnection.Nickname )
                    {
                        continue;
                    }

                    EmoteCallback?.Invoke( (byte)messages[ i ] );
                }
                break;
            }
        }

        public override int GetAttendeesCount()
        {
            int count = m_Connection.GetEmoteChannelUserCount();
            if( IsSpeakerPresent() )
            {
                --count;
            }
            return count;
        }

        public override void UpdateSpeakerPing()
        {
            if( Time.realtimeSinceStartup < m_NextPing )
            {
                return;
            }

            m_NextPing = Time.realtimeSinceStartup + PingDelay;
            m_Connection.SpeakerSendPing();
        }

        void OnGetSpeakerPing()
        {
            m_LastReceivedPing = Time.realtimeSinceStartup;
        }

        public override void SendSpeakerCustomization( ExitGames.Client.Photon.Hashtable data )
        {
            m_Connection.SendSpeakerCustomization( data );
        }
        public override void SendSpeakerXRUpdate( ExitGames.Client.Photon.Hashtable data )
        {
            m_Connection.SendSpeakerXRUpdate( data );
        }

        public override void SendScoreBoard( ExitGames.Client.Photon.Hashtable data )
        {
            //m_Connection.SendScoreBoard( data );
        }

        public override void SendEmote( byte emote )
        {
            m_Connection.SendEmote( emote );
        }
        public override void SendPresentationUri( int uri )
        {
            // m_Connection.SendPresentationUri( uri );
        }

        public override void UpdateTimer( int timer ){
             m_Connection.UpdateTimer( timer );
        }
    }
}
