
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Chat;
using ExitGames.Client.Photon;
using System.Linq;

namespace SpaceHub.Groups
{
    public class UserManager : MonoBehaviour
    {
        public RectTransform UserListParent;
        public GameObject UserButtonPrefab;

        Dictionary<string, User> m_Users = new Dictionary<string, User>();

        GroupsManager m_Manager;
        public MinimizeElement Minimizer;

        public UnityAction<string> UserClickedCallback;
        public UnityAction UserListUpdatedCallback;

        public Dictionary<string, UserListButton> m_UserListButtons = new Dictionary<string, UserListButton>();

        private void Awake()
        {
            m_Manager = GetComponent<GroupsManager>();
        }

        public void OnChannelPropertiesChanged( Dictionary<object, object> properties )
        {
            foreach( var pair in properties )
            {
                var key = pair.Key as string;

                if( key.StartsWith( VoiceManager.VoicePropertyPrefix ) == false )
                {
                    continue;
                }

                string userId = key.Substring( VoiceManager.VoicePropertyPrefix.Length );
                if( m_UserListButtons.ContainsKey( userId ) == false )
                {
                    continue;
                }

                var button = m_UserListButtons[ userId ];
                button.SetVoiceIconVisible( (bool)pair.Value );

            }
        }

        public void ShowPrivateChatRequests()
        {
            m_Manager.ChannelManager.SwitchChannel( "", true );
        }

        public void UpdateUserList( string channelName, bool requestList )
        {
            Helper.DestroyAllChildren( UserListParent );
            m_UserListButtons.Clear();

            if( requestList )
            {
                UpdateUserListForPrivateRequests();
            }
            else
            {
                UpdateUserListForChannel( channelName );
            }
        }

        void UpdateUserListForPrivateRequests()
        {
            Minimizer?.SetEnabledAndVisible( true, true );
            foreach( var pair in m_Manager.ChannelManager.ChannelDatas )
            //foreach( var pair in m_Manager.Client.PrivateChannels )
            {
                if ( pair.Value.IsPrivate == false )
                {
                    continue;
                }
                if( m_Manager.Permissions.IsPrivateChatConfirmed( pair.Key ) == true )
                {
                    continue;
                }

                var data = pair.Value;
                var user = m_Manager.UserManager.GetOrCreateUser( data.PrivateChannelOtherUser );
                var go = Instantiate( UserButtonPrefab, UserListParent );

                var button = go.GetComponent<UserListButton>();
                m_UserListButtons.Add( data.PrivateChannelOtherUser, button );
                button.SetUser( user, OnPrivateChatUserClicked );
            }
        }

        void OnPrivateChatUserClicked( User user )
        {
            Debug.Log( "user clicked: " + user.Nickname + ", " + user.SenderId );
            m_Manager.ChannelManager.SwitchChannel( VoiceManager.GetVoiceRoomName( m_Manager.Client.UserId, user.SenderId ), true );
        }

        void UpdateUserListForChannel( string channelName )
        {
            if( m_Manager.ChannelManager.CurrentChannel == null )
            {
                Minimizer?.SetEnabledAndVisible( false, false );
                return;
            }

            if( string.IsNullOrEmpty( channelName ) || m_Manager.ChannelManager.CurrentChannel.Name == channelName )
            {
                foreach( var userId in m_Manager.ChannelManager.CurrentChannel.Channel.Subscribers )
                {
                    var user = m_Manager.UserManager.GetOrCreateUser( userId );

                    var go = Instantiate( UserButtonPrefab, UserListParent );
                    var button = go.GetComponent<UserListButton>();
                    m_UserListButtons.Add( userId, button );

                    button.SetUser( user, OnUserClicked );
                }

                bool showUsers = m_Manager.ChannelManager.CurrentChannel.Channel.PublishSubscribers;
                Minimizer?.SetEnabledAndVisible( showUsers, false );
            }

            UserListUpdatedCallback?.Invoke();
        }

        public User FindUserWithId( string userId )
        {
            var pair = m_Users.Where( item => item.Value.SenderId == userId );
            if ( pair.Count() > 0 )
            {
                return pair.First().Value;
            }
            return null;
        }

        public void UserSubscribed( string channel, string senderId )
        {
            var user = GetOrCreateUser( senderId );
            UpdateUserList( channel, false );
        }

        public void UserUnsubscribed( string channel, string senderId )
        {
            var user = GetOrCreateUser( senderId );
            UpdateUserList( channel, false );
        }

        public void OnUserClicked( User user )
        {
            UserClickedCallback?.Invoke( user.SenderId );
        }

        public User GetOrCreateUser( string senderId )
        {
            if( string.IsNullOrEmpty( senderId ) )
            {
                Debug.LogError( "SenderID is null or empty. Can't create user. Creating Dummy" );
                GetOrCreateUser( Helper.GetAuthName( "dummyId", "dummyNickname" ) );
            }

            if( m_Users.ContainsKey( senderId ) == false )
            {
                m_Users.Add( senderId, new User( senderId ) );
            }
            return m_Users[ senderId ];
        }

        public void InviteToGroupChannel( string userId, string channelName, bool isPrivate )
        {
            var table = new Hashtable();
            table.Add( "type", MessageType.InviteToGroup );
            table.Add( "channelName", channelName );
            table.Add( "isPrivate", isPrivate );
            m_Manager.Client.SendPrivateMessage( userId, table );
        }

        public void InviteToCurrentChannel( string userId )
        {
            var channel = m_Manager.ChannelManager.CurrentChannel;
            if( channel == null || string.IsNullOrEmpty( channel.Name ) )
            {
                Debug.LogWarning( "No Channel selected to invite to" );
                return;
            }
            InviteToGroupChannel( userId, channel.Name, channel.IsPrivate );
        }
    }
}
