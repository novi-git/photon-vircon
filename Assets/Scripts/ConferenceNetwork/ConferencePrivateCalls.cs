using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceHub.Conference
{

    public class ConferencePrivateCalls : MonoBehaviour, IOnEventCallback
    {
        LoadBalancingClient m_Client;
        public List<Player> IncomingCallRequests { get; private set; } = new List<Player>();
        public List<Player> OutgoingCallRequests { get; private set; } = new List<Player>();

        public UnityAction<List<Player>> IncomingCallsListChangedCallback;
        public UnityAction<List<Player>> OutgoingCallsListChangedCallback;

        string m_LastPrivateCallRoomName;

        public void SetClient( LoadBalancingClient client )
        {
            m_Client = client;
            m_Client.AddCallbackTarget( this );
        }

        void OnLeftVoiceRoom( string roomName )
        {
            if( roomName == m_LastPrivateCallRoomName )
            {
                StopCall();
            }
        }

        void OnPlayerLeftVoiceRoom( Player player, Room room )
        {
            if( room.Name == m_LastPrivateCallRoomName && room.PlayerCount <= 1 )
            {
                if( PrivateCallPopup.Instance?.IsOpen() == true )
                {
                    StopCall();
                    string partnerName = "Partner";
                    if( string.IsNullOrWhiteSpace( player.GetNickNameProperty() ) == false )
                    {
                        partnerName = player.GetNickNameProperty();
                    }

                    MessagePopup.Show( $"{partnerName} has left the call." );
                }
            }
        }

        public void StopCall()
        {
            ConferenceVoiceConnection.Instance.LeaveRoom();
            PrivateCallPopup.Instance.Close();

            PlayerLocalChatbubble.Instance.ConnectToCurrentBubbleVoiceRoom();
        }

        void OnDestroy()
        {
            if( m_Client != null )
            {
                m_Client.RemoveCallbackTarget( this );
            }
        }

        public void SendCallRequestTo( Player player )
        {
            if( SendRaiseEvent( player, ConferenceEvent.RequestPrivateCall ) )
            {
                AddOutgoingCallRequest( player );
            }
        }

        public void CancelCallRequestTo( Player player )
        {
            if( OutgoingCallRequests.Contains( player ) == false )
            {
                return;
            }

            if( SendRaiseEvent( player, ConferenceEvent.CancelPrivateCallRequest ) )
            {
                RemoveOutgoingCallRequest( player );
            }
        }

        public void AcceptCallFrom( Player player )
        {
            if( IncomingCallRequests.Contains( player ) == false )
            {
                return;
            }

            m_LastPrivateCallRoomName = "PrivateCall-" + m_Client.LocalPlayer.ActorNumber + "-" + player.ActorNumber + "-" + Random.Range( 100000, 999999 );

            if( SendRaiseEvent( player, ConferenceEvent.AcceptPrivateCall, m_LastPrivateCallRoomName ) )
            {
                RemoveIncomingCallRequest( player );
                JoinPrivateCall( m_LastPrivateCallRoomName, player );
            }
        }

        public void DeclineCallFrom( Player player )
        {
            if( IncomingCallRequests.Contains( player ) == false )
            {
                return;
            }

            if( SendRaiseEvent( player, ConferenceEvent.DeclinePrivateCall ) )
            {
                RemoveIncomingCallRequest( player );
            }
        }

        public void OnEvent( EventData photonEvent )
        {
            if( photonEvent.Code < ConferenceEvent.RequestPrivateCall || photonEvent.Code > ConferenceEvent.CancelPrivateCallRequest )
            {
                return;
            }

            Player caller = m_Client.CurrentRoom.GetPlayer( photonEvent.Sender );
            Debug.Log( "Call event " + photonEvent.Code + " from caller " + caller.NickName );

            switch( photonEvent.Code )
            {
            case ConferenceEvent.RequestPrivateCall:
                if( IncomingCallRequests.Contains( caller ) == false )
                {
                    AddIncomingCallRequest( caller );
                    PrivateCallPopup.Instance.OpenCallRequestFrom( caller );
                }
                break;
            case ConferenceEvent.AcceptPrivateCall:
                if( OutgoingCallRequests.Contains( caller ) == true )
                {
                    RemoveOutgoingCallRequest( caller );
                    m_LastPrivateCallRoomName = (string)photonEvent.CustomData;

                    JoinPrivateCall( m_LastPrivateCallRoomName, caller );                    
                }
                break;
            case ConferenceEvent.DeclinePrivateCall:
                RemoveOutgoingCallRequest( caller );
                break;
            case ConferenceEvent.CancelPrivateCallRequest:
                RemoveIncomingCallRequest( caller );
                break;
            }
        }

        void JoinPrivateCall( string roomName, Player otherPlayer )
        {
            ConferenceVoiceConnection.Instance.PlayerLeftRoomCallback -= OnPlayerLeftVoiceRoom;
            ConferenceVoiceConnection.Instance.PlayerLeftRoomCallback += OnPlayerLeftVoiceRoom;

            ConferenceVoiceConnection.Instance.LeftRoomCallback -= OnLeftVoiceRoom;
            ConferenceVoiceConnection.Instance.LeftRoomCallback += OnLeftVoiceRoom;

            ConferenceVoiceConnection.Instance.JoinRoom( roomName );
            PrivateCallPopup.Instance.OpenPrivateCallWith( otherPlayer );
        }

        bool SendRaiseEvent( Player targetPlayer, byte eventCode, object customEventContent = null )
        {
            if( m_Client == null )
            {
                Debug.LogError( "Can't send private call request. No client is set" );
                return false;
            }

            var options = new RaiseEventOptions();
            options.TargetActors = new int[] { targetPlayer.ActorNumber };

            m_Client.OpRaiseEvent(
                eventCode,
                customEventContent,
                options,
                SendOptions.SendReliable
                );

            return true;
        }

        void AddIncomingCallRequest( Player caller )
        {
            if( IncomingCallRequests.Contains( caller ) )
            {
                IncomingCallRequests.Remove( caller );
            }
            IncomingCallRequests.Add( caller );
            IncomingCallsListChangedCallback?.Invoke( IncomingCallRequests );
        }

        void AddOutgoingCallRequest( Player target )
        {
            if ( OutgoingCallRequests.Contains(target))
            {
                OutgoingCallRequests.Remove( target );
            }
            OutgoingCallRequests.Add( target );
            OutgoingCallsListChangedCallback?.Invoke( OutgoingCallRequests );
        }

        void RemoveIncomingCallRequest( Player caller )
        {
            IncomingCallRequests.Remove( caller );
            IncomingCallsListChangedCallback?.Invoke( IncomingCallRequests );
        }

        void RemoveOutgoingCallRequest( Player target )
        {
            OutgoingCallRequests.Remove( target );
            OutgoingCallsListChangedCallback?.Invoke( OutgoingCallRequests );
        }
    }
}