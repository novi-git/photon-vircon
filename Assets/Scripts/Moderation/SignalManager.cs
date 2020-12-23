using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SpaceHub.Conference
{

    public class SignalManager : MonoBehaviour
    {
        public static readonly string SignalChannelName = "SignalChannel";
        public static SignalManager Instance;

        Groups.GroupsManager GroupsManager { get { return PlayerLocal.Instance.ChatClient.GroupsManager; } }
        public enum SignalType
        {
            Teleport,
            RequestTeleportPosition,
            TeleportToPosition,
            Kick,
        }

        Dictionary<SignalType, UnityAction<Hashtable, string>> m_Callbacks = new Dictionary<SignalType, UnityAction<Hashtable, string>>();

        private void Awake()
        {
            Instance = this;

            RegisterDefaultCallbacks();
        }

        public void OnReceiveSignal( Hashtable table, string senderId )
        {
            var type = (SignalType)table[ "signalType" ];
            string receiver = table.ContainsKey( "signalReceiver" ) ? (string)table[ "signalReceiver" ] : null;
            if( receiver != null && receiver != PlayerLocal.Instance.ChatClient.m_Client.UserId )
            {
                Debug.LogWarning( "Receive Signal " + type.ToString() + " for different userId:" );
                return;
            }
            Debug.Log( "Receive Signal " + type.ToString() );


            if( m_Callbacks.ContainsKey( type ) )
            {
                m_Callbacks[ type ]?.Invoke( ( table.ContainsKey( "signalMessage" ) ? (Hashtable)table[ "signalMessage" ] : null ), senderId );

            }
        }

        public void SendPrivateSignal( SignalType type, string receiver, Hashtable message )
        {
            var table = new Hashtable();
            table.Add( "customType", ConferenceChatListener.CustomMessageType.signal );
            table.Add( "signalType", type );
            table.Add( "signalReceiver", receiver );
            table.Add( "signalMessage", message );

            GroupsManager.SendCustomPrivateMessage( receiver, table );
        }

        public void SendSignal( SignalType type, Hashtable table )
        {
            table.Add( "customType", ConferenceChatListener.CustomMessageType.signal );
            table.Add( "signalType", type );
            table.Add( "signalMessage", table );
            GroupsManager.ChannelManager.SendCustomMessage( SignalChannelName, table );
        }

        public void RegisterCallback( SignalType type, UnityAction<Hashtable, string> callback )
        {
            if( m_Callbacks.ContainsKey( type ) == false )
            {
                m_Callbacks.Add( type, default );
            }

            m_Callbacks[ type ] -= callback;
            m_Callbacks[ type ] += callback;
        }

        public void UnregisterCallback( SignalType type, UnityAction<Hashtable, string> callback )
        {
            if( m_Callbacks.ContainsKey( type ) )
            {
                m_Callbacks[ type ] -= callback;
            }
        }

        void RegisterDefaultCallbacks()
        {
            RegisterCallback( SignalType.RequestTeleportPosition, SendSignalTeleportToPosition );
            RegisterCallback( SignalType.TeleportToPosition, OnSignalTeleportToPosition );
            RegisterCallback( SignalType.Kick, OnKickReceived );
        }

        public void OnKickReceived( Hashtable table, string senderId )
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
        }

        public void SendPullUserToMyPosition( string targetSenderId )
        {
            SendSignalTeleportToPosition( null, targetSenderId );
        }
        public void SendRequestUserPositionAndTeleport( string targetSenderId )
        {
            SendPrivateSignal( SignalManager.SignalType.RequestTeleportPosition, targetSenderId, null );
        }

        void SendSignalTeleportToPosition( Hashtable table, string senderId )
        {
            var sendTable = new Hashtable();
            sendTable.Add( "room", ConferenceRoomManager.Instance.CurrentRoomName );
            sendTable.Add( "position", PlayerLocal.Instance.CurrentPosition );
            SendPrivateSignal( SignalType.TeleportToPosition, senderId, sendTable );
            PlayerLocal.Instance.ForceSendCurrentPositionAndRotationWithHighAccuracy( 0 );
        }

        public void OnSignalTeleportToPosition( Hashtable table, string senderId )
        {
            var roomName = table[ "room" ] as string;
            var position = (Vector3)table[ "position" ];


            if( string.IsNullOrEmpty( roomName ) == false && roomName != ConferenceRoomManager.Instance.CurrentRoomName )
            {
                ConferenceRoomManager.LoadRoom( roomName, null, null,
                    () =>
                    {
                        PlayerLocal.Instance.GoToAndLook( position, true, false );
                        PlayerLocal.Instance.ForceSendCurrentPositionAndRotationWithHighAccuracy( 0 );
                    }
                );
            }
            else
            {
                PlayerLocal.Instance.GoToAndLook( position, true, false );
                PlayerLocal.Instance.ForceSendCurrentPositionAndRotationWithHighAccuracy( 0 );
            }

        }


    }
}
