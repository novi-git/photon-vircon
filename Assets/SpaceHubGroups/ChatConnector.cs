using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.Events;

namespace SpaceHub.Groups
{

    public class ChatConnector : MonoBehaviour, IChatClientListener
    {
        public ChatClient m_Client { get; private set; }
        public static string authName { get; private set; }
        public UnityAction<string, string[], object[]> OnGetMessagesCallback;

        public List<IChatClientListener> Listeners = new List<IChatClientListener>();

        public string AppId;
        public string AppVersion;

        public bool m_ReconnectOnDisconnect = true;

        Dictionary<string, CallbackData> m_Callbacks = new Dictionary<string, CallbackData>();

        class CallbackData
        {
            public List<UnityAction<string, object>> Callbacks = new List<UnityAction<string, object>>();
        }

        public Groups.GroupsManager GroupsManager { get; private set; }

        public void Connect( string userId, string nickname, string region )
        {
            
            Debug.Log( "Chat Connector - connecting with " + userId + ", " + nickname + " in " + region );
            authName = SpaceHub.Groups.Helper.GetAuthName( userId, nickname );

            m_Client = new ChatClient( this );
            m_Client.ChatRegion = region;

            m_Client.Connect( AppId, AppVersion, new AuthenticationValues( authName ) );

            GroupsManager.SetChatClient( m_Client );
            Listeners.Add( GroupsManager );

            Application.quitting += ApplicationQuits;
        }

        void ApplicationQuits()
        {
            m_ReconnectOnDisconnect = false;
        }

        public void SetGroupsManager( GroupsManager groupsManager )
        {
            GroupsManager = groupsManager;
        }


        private void OnDestroy()
        {
            Listeners.Remove( GroupsManager );
        }

        public void RegisterCallback( string roomName, UnityAction<string, object> callback )
        {
            if( m_Callbacks.ContainsKey( roomName ) == false )
            {
                m_Callbacks.Add( roomName, new CallbackData() );
                //GroupsManager.ChannelManager.JoinChannel( roomName, true );
                //StartCoroutine( SubscribeWhenReadyRoutine( roomName, options ) );
            }

            if( m_Callbacks[ roomName ].Callbacks.Contains( callback ) == false )
            {
                m_Callbacks[ roomName ].Callbacks.Add( callback );
            }
        }



        public void UnregisterCallback( string roomName, UnityAction<string, object> callback )
        {
            if( string.IsNullOrEmpty( roomName ) == false && m_Callbacks.ContainsKey( roomName ) )
            {
                if( m_Callbacks[ roomName ].Callbacks.Contains( callback ) )
                {
                    m_Callbacks[ roomName ].Callbacks.Remove( callback );
                }

                if( m_Callbacks[ roomName ].Callbacks.Count == 0 )
                {
                    //GroupsManager.ChannelManager.LeaveChannel( roomName );
                    //m_Client.Unsubscribe( new string[] { roomName } );
                    m_Callbacks.Remove( roomName );
                }
            }
        }

        void Update()
        {
            if( m_Client == null )
            {
                return;
            }

            m_Client.Service();
        }

        public void DebugReturn( DebugLevel level, string message )
        {
            Debug.Log( "Chat:" + message );
        }

        public void OnChatStateChange( ChatState state )
        {
            Debug.Log( "OnChatStateChange " + state );
            foreach( var listener in Listeners )
            {
                listener.OnChatStateChange( state );
            }
        }

        public void OnConnected()
        {
            Debug.Log( "Chat Client Connected" );
            foreach( var listener in Listeners )
            {
                listener.OnConnected();
            }
        }

        public void OnDisconnected()
        {
            Debug.Log( "Chat Client Disconnected" );
            foreach( var listener in Listeners )
            {
                listener.OnDisconnected();
            }

            if( m_ReconnectOnDisconnect  )
            {
                m_Client.Connect( AppId, AppVersion, new AuthenticationValues( authName ) );
            }
        }

        public void OnGetMessages( string channelName, string[] senders, object[] messages )
        {
            foreach( var listener in Listeners )
            {
                listener.OnGetMessages( channelName, senders, messages );
            }
            OnGetMessagesCallback?.Invoke( channelName, senders, messages );

            if( m_Callbacks.ContainsKey( channelName ) )
            {
                var list = m_Callbacks[ channelName ].Callbacks;
                foreach( var entry in list )
                {
                    for( int i = 0; i < senders.Length; ++i )
                    {
                        entry?.Invoke( senders[ i ], messages[ i ] );
                    }
                }
            }
        }

        public void OnPrivateMessage( string sender, object message, string channelName )
        {
            foreach( var listener in Listeners )
            {
                listener.OnPrivateMessage( sender, message, channelName );
            }
        }

        public void OnStatusUpdate( string user, int status, bool gotMessage, object message )
        {
            foreach( var listener in Listeners )
            {
                listener.OnStatusUpdate( user, status, gotMessage, message );
            }
            Debug.Log( "Chat Client OnStatusUpdate" );
        }

        public void OnSubscribed( string[] channels, bool[] results )
        {
            foreach( var listener in Listeners )
            {
                listener.OnSubscribed( channels, results );
            }
            Debug.Log( "Chat Client OnSubscribed: " + string.Join( ", ", channels ) );
        }

        public void OnUnsubscribed( string[] channels )
        {
            foreach( var listener in Listeners )
            {
                listener.OnUnsubscribed( channels );
            }
            Debug.Log( "Chat Client OnUnsubscribed: " + string.Join( ", ", channels ) );
        }

        public void OnUserSubscribed( string channel, string user )
        {
            foreach( var listener in Listeners )
            {
                listener.OnUserSubscribed( channel, user );
            }
            Debug.Log( "Chat Client OnUserSubscribed " + channel + ", " + user );
        }

        public void OnUserUnsubscribed( string channel, string user )
        {
            foreach( var listener in Listeners )
            {
                listener.OnUserUnsubscribed( channel, user );
            }
            Debug.Log( "Chat Client OnUserUnsubscribed " + channel + ", " + user );
        }

        public void OnChannelPropertiesChanged( string channel, string userId, Dictionary<object, object> properties )
        {
            foreach( var listener in Listeners )
            {
                listener.OnChannelPropertiesChanged( channel, userId, properties );
            }
            Debug.Log( "Chat Client OnChannelPropertiesChanged " + channel + ", " + userId );
        }
        public void OnErrorInfo( string channel, string error, object data )
        {
            foreach( var listener in Listeners )
            {
                listener.OnErrorInfo( channel, error, data );
            }
            Debug.Log( "Chat Client OnErrorInfo " + channel + ", " + error );
        }

        public void OnUserPropertiesChanged( string channel, string targetUserId, string senderUserId, Dictionary<object, object> properties )
        {
            foreach( var listener in Listeners )
            {
                listener.OnUserPropertiesChanged( channel, targetUserId, senderUserId, properties );
            }
            Debug.Log( "Chat Client OnUserPropertiesChanged " + channel + ", " + targetUserId + ", sender:" + senderUserId );
        }
    }
}