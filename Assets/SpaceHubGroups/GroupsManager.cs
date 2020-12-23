
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Chat;
using ExitGames.Client.Photon;

using Photon.Voice.Unity;

namespace SpaceHub.Groups
{

    public enum MessageType
    {
        Chat = 0,
        InviteToGroup = 1,
        Custom = 2,
    }

    [RequireComponent( typeof( ChannelManager ) )]
    [RequireComponent( typeof( UserManager ) )]
    [RequireComponent( typeof( InvitesManager ) )]
    [RequireComponent( typeof( ChatInput ) )]
    [RequireComponent( typeof( VoiceManager ) )]
    public class GroupsManager : MonoBehaviour, IChatClientListener
    {
        public UserManager UserManager { get; private set; }
        public ChannelManager ChannelManager { get; private set; }
        public InvitesManager InvitesManager { get; private set; }
        public ChatInput ChatInput { get; private set; }
        public VoiceManager VoiceManager { get; private set; }

        public ChatClient Client { get; private set; }

        public MinimizeElement MinimizerChatLog;
        public MinimizeElement MinimizerChatInput;

        public UnityAction<string, Hashtable> ReceiveCustomPrivateMessageCallback;


        public ChannelPermissions Permissions;

        public void SetChatClient( ChatClient client )
        {
            Client = client;
        }


        private void Awake()
        {
            UserManager = GetComponent<UserManager>();
            ChannelManager = GetComponent<ChannelManager>();
            InvitesManager = GetComponent<InvitesManager>();
            ChatInput = GetComponent<ChatInput>();
            VoiceManager = GetComponent<VoiceManager>();

            Permissions = new ChannelPermissions( this );
        }

        public void OnUserSubscribed( string channelName, string senderId )
        {
            UserManager.UserSubscribed( channelName, senderId );
        }

        public void OnUserUnsubscribed( string channelName, string senderId )
        {
            UserManager.UserUnsubscribed( channelName, senderId );
        }

        public void OnPrivateMessage( string sender, object message, string channelName )
        {

            var table = message as Hashtable;
            var messageType = (MessageType)table[ "type" ];

            switch( messageType )
            {
            case MessageType.InviteToGroup:

                if( Permissions.IsUserBlocked( sender ) )
                {
                    break;
                }

                if( sender == Client.UserId )
                {
                    break;
                }
                var inviteChannel = table[ "channelName" ] as string;
                var isPrivate = (bool)table[ "isPrivate" ];
                if( isPrivate )
                {
                    ChannelManager.TryGetOrCreatePrivateChannelData( sender, out var data );
                    ChannelManager.UnconfirmedPrivateChatsButton?.IncreaseUnreadMessages();
                }
                else
                {
                    InvitesManager.AddInvite( sender, inviteChannel, isPrivate );
                }

                break;

            /*
        case MessageType.InviteConfirm:
            if( sender == Client.UserId )
            {
                break;
            }
            ChannelManager.CreatePrivateChannelIfNeeded( sender );
            break;*/
            /*
        case MessageType.Chat:
                //if( sender != Client.UserId )
                {
                    ChannelManager.TryGetOrCreatePrivateChannelData( sender, out var data );
                }

                ChannelManager.ReceiveMessage( channelName, sender, table[ "text" ] as string );
            break;
                */
            case MessageType.Custom:
                ReceiveCustomPrivateMessageCallback?.Invoke( sender, (Hashtable)table[ "content" ] );
                break;
            default:
                Debug.LogError( "Could not handle private chat message type " + messageType );
                break;
            }
        }



        public void OnStatusUpdate( string user, int status, bool gotMessage, object message )
        {
        }

        public void OnGetMessages( string channelName, string[] senders, object[] messages )
        {
            for( int i = 0; i < senders.Length; ++i )
            {
                if( Permissions.IsUserBlocked( senders[ i ] ) )
                {
                    continue;
                }

                var table = messages[ i ] as Hashtable;
                if( table.ContainsKey( "type" ) == false )
                {
                    Debug.LogError( "Could not find key \"type\" in Message." );
                    continue;
                }
                var messageType = (MessageType)table[ "type" ];
                switch( messageType )
                {
                case MessageType.Chat:
                    ChannelManager.ReceiveMessage( channelName, senders[ i ], table[ "text" ] as string );
                    break;
                case MessageType.Custom:
                    ChannelManager.ReceiveCustomMessage( channelName, senders[ i ], table );
                    break;
                default:
                    Debug.LogError( "Could not handle chat message type " + messageType );
                    break;
                }
            }
        }

        public void SendCustomPrivateMessage( string receiverId, object message )
        {
            var table = new Hashtable();
            table.Add( "type", MessageType.Custom );
            table.Add( "content", message );
            Client.SendPrivateMessage( receiverId, table );
        }

        public void OnSubscribed( string[] channels, bool[] results )
        {
            ChannelManager.SwitchChannel( channels[ 0 ] );
            ChannelManager.OnSubscribe( channels );
        }

        public void OnUnsubscribed( string[] channels )
        {
            ChannelManager.OnUnSubscribe( channels );
        }

        public void DebugReturn( DebugLevel level, string message ) { }
        public void OnDisconnected() { }
        public void OnConnected() { }
        public void OnChatStateChange( ChatState state ) { }



        public void OnChannelPropertiesChanged( string channel, string userId, Dictionary<object, object> properties )
        {
            if( ChannelManager.GetCurrentChannelName() == channel )
            {
                UserManager.OnChannelPropertiesChanged( properties );
            }

            VoiceManager?.OnChannelPropertiesChanged( channel, userId, properties );
        }

        public void OnUserPropertiesChanged( string channel, string targetUserId, string senderUserId, Dictionary<object, object> properties ) { }
        public void OnErrorInfo( string channel, string error, object data ) { }


    }
}
