
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Photon.Chat;
using ExitGames.Client.Photon;
using System.Text;

namespace SpaceHub.Groups
{
    public class ChannelData
    {
        public ChannelListButton Button;
        public ChatChannel Channel;

        public string Name;
        public string PrivateChannelOtherUser;
        public string VoiceRoomName;
        public bool IsPrivate;


        public void IncreaseUnreadMessages()
        {
            if( Button != null )
            {
                Button.IncreaseUnreadMessages();
            }
        }

        public void CreateButton( GameObject prefab, Transform parent, UnityAction<ChannelData> onClickCallback )
        {
            if( Button != null )
            {
                return;
            }
            var go = GameObject.Instantiate( prefab, parent );
            Button = go.GetComponent<ChannelListButton>();
            Button.Initialize( this, onClickCallback );// ( string channelname ) => { SwitchChannel( channelname, false ); } );
        }
        public void RemoveButton()
        {
            if( Button != null )
            {
                GameObject.Destroy( Button.gameObject );
            }
            Button = null;
        }
    }

    public class ChannelManager : MonoBehaviour
    {
        public delegate string NameFromChannelDelegate( ChatChannel channel );
        public NameFromChannelDelegate GetChannelNameOverride;

        public delegate string GetTextForCustomMessageDelegate( ChannelData channelData, string senderId, Hashtable table );
        public GetTextForCustomMessageDelegate GetTextForCustomMessageOverride;

        public delegate void CreateChannelButtonDelegate( ChannelData channelData );
        public CreateChannelButtonDelegate CreateChannelButtonOverride;

        StringBuilder m_Sb = new StringBuilder();
        GroupsManager m_Manager;
        Dictionary<string, ChannelData> m_ChannelDatas = new Dictionary<string, ChannelData>();

        public Dictionary<string, ChannelData> ChannelDatas { get { return m_ChannelDatas; } }


        public TextMeshProUGUI[] ChatlogText;
        public ChatTopBar TopBar;

        public ChannelListButton UnconfirmedPrivateChatsButton;
        //public TextMeshProUGUI ChannelHeadline;

        public RectTransform ChannelListParent;
        public GameObject ChannelButtonPrefab;
        public MinimizeElement Minimizer;

        public ChannelData CurrentChannel { get; private set; }
        public string PrivateMessageTarget { get; private set; }

        //public bool UpdateChannelNamesWhenUsersChange = true;

        public UnityAction<ChannelData> ChannelLeftCallback;
        public UnityAction<ChannelData> ChannelJoinedCallback;


        public UnityAction<ChannelData> CreateChannelButtonCallback;
        public UnityAction<ChannelData> DeleteChannelButtonCallback;

        public UnityAction LeaveCurrentChannelCallback;

        public UnityAction<string> SelectChannelCallback;


        public UnityAction<ChannelData, string, Hashtable> ReceiveCustomMessageCallback;

        private void Awake()
        {
            m_Manager = GetComponent<GroupsManager>();
            Helper.DestroyAllChildren( ChannelListParent );
        }

        public void SendMessageToCurrentChannel( string message )
        {
            if( CurrentChannel == null )
            {
                Debug.LogWarning( "NoChannel Selected to publish message in." );
                return;
            }

            if( CurrentChannel.IsPrivate && CurrentChannel.Channel.Subscribers.Count <= 1 && CurrentChannel.Channel.MessageCount == 0 )
            {
                m_Manager.UserManager.InviteToCurrentChannel( CurrentChannel.PrivateChannelOtherUser );
            }

            Hashtable table = new Hashtable();
            table.Add( "type", MessageType.Chat );
            table.Add( "text", message );
            m_Manager.Client.PublishMessage( CurrentChannel.Name, table );
        }

        public void SendCustomMessageToCurrentChannel( Hashtable table )
        {
            if( CurrentChannel == null )
            {
                Debug.LogWarning( "NoChannel Selected to publish message in." );
                return;
            }

            SendCustomMessage( CurrentChannel.Name, table );
        }

        public void SendCustomMessage( string channelName, Hashtable table )
        {
            var sendTable = new Hashtable();
            sendTable.Add( "type", MessageType.Custom );
            sendTable.Add( "content", table );
            m_Manager.Client.PublishMessage( channelName, sendTable );
        }

        public void ReceiveCustomMessage( string channelId, string senderId, Hashtable table )
        {
            Debug.Log( "Received custom message " + channelId + " senderid " + senderId );
            var content = (Hashtable)table[ "content" ];
            ReceiveCustomMessageCallback?.Invoke( GetChannelData( channelId ), senderId, content );

            if( CurrentChannel != null && CurrentChannel.Name == channelId && GetTextForCustomMessageOverride != null )
            {
                var text = GetTextForCustomMessageOverride( GetChannelData( channelId ), senderId, content );
                if( text != null )
                {
                    AddMessage( text );
                    UpdateText();
                }
            }
        }

        public void ReceiveMessage( string channelId, string userId, string message )
        {
            if( CurrentChannel == null || CurrentChannel.Name != channelId )
            {
                if( m_ChannelDatas.ContainsKey( channelId ) )
                {
                    if( m_ChannelDatas[ channelId ].Button == null )
                    {
                        UnconfirmedPrivateChatsButton.IncreaseUnreadMessages();
                    }
                    else
                    {
                        m_ChannelDatas[ channelId ].IncreaseUnreadMessages();
                    }
                }
                return;
            }

            AddMessage( userId, message );
            UpdateText();
        }

        public ChatChannel GetChannel( string channelName, bool isPrivate )
        {
            m_Manager.Client.TryGetChannel( channelName, isPrivate, out var result );
            return result;
        }

        public ChannelData GetChannelData( string channelName )
        {
            if( m_ChannelDatas.ContainsKey( channelName ) == false )
            {
                return null;
            }
            return m_ChannelDatas[ channelName ];
        }

        public void UpdateCallIcons( string currentVoiceCall )
        {
            foreach( var pair in m_ChannelDatas )
            {
                if( pair.Value.Button == null )
                {
                    continue;
                }

                var channel = pair.Value.Channel;
                bool callActive = false;
                /*
                if( channel.IsPrivate )
                {
                    callActive = false;
                }
                else*/
                if( channel.PublishSubscribers )
                {
                    foreach( var sub in channel.Subscribers )
                    {
                        if( channel.TryGetCustomChannelProperty<bool>( VoiceManager.VoicePropertyPrefix + sub, out var userInCall ) && userInCall )
                        {
                            callActive = true;
                            break;
                        }
                    }
                }
                pair.Value.Button.SetVoiceIcon( callActive, pair.Value.VoiceRoomName == currentVoiceCall );
            }
        }

        public void OnSubscribe( string[] channels )
        {
            string channelToSwitchTo = "";
            foreach( var name in channels )
            {
                if( m_Manager.Client.TryGetChannel( name, out var channel ) )
                {
                    var data = GetOrCreateChannelData( channel );
                    if( data.IsPrivate && m_Manager.Permissions.IsPrivateChatConfirmed( name ) == false )
                    {
                        continue;
                    }
                    CreateChannelButton( data );
                    ChannelJoinedCallback?.Invoke( data );
                    channelToSwitchTo = data.Name;
                }
            }

            if( string.IsNullOrEmpty( channelToSwitchTo ) == false )
            {
                //UpdateChannelButtons();
                SwitchChannel( channelToSwitchTo );
            }

        }

        public void OnUnSubscribe( string[] channels )
        {
            foreach( var key in channels )
            {
                if( m_ChannelDatas.ContainsKey( key ) && m_ChannelDatas[ key ] != null )
                {
                    var data = m_ChannelDatas[ key ];
                    if( data.Button != null )
                    {
                        RemoveChannelButton( data );
                    }

                    ChannelLeftCallback?.Invoke( data );

                    if( CurrentChannel != null && CurrentChannel.Name == key )
                    {
                        OnLeaveCurrentChannel();
                    }

                    m_ChannelDatas.Remove( key );
                }
            }
        }


        void OnLeaveCurrentChannel()
        {
            Debug.Log( "Leaving current channel" );
            m_Sb.Clear();
            UpdateText();
            m_Manager.UserManager.UpdateUserList( null, false );
            LeaveCurrentChannelCallback?.Invoke();
        }

        void RemoveChannelButton( ChannelData data )
        {
            DeleteChannelButtonCallback?.Invoke( data );
            data.RemoveButton();
        }

        public void SendOpenLocalPrivateChat( string senderId )
        {
            m_Manager.Permissions.ConfirmPrivateChat( VoiceManager.GetVoiceRoomName( m_Manager.Client.UserId, senderId ) );
            JoinPrivateChannel( senderId );
        }

        public bool TryGetOrCreatePrivateChannelData( string senderId, out ChannelData data )
        {
            string channelName = VoiceManager.GetVoiceRoomName( senderId, m_Manager.Client.UserId );
            if( m_Manager.Client.TryGetChannel( channelName, out var channel ) )
            {
                data = GetOrCreateChannelData( channel );
                return true;
            }
            else
            {
                JoinPrivateChannel( senderId );
                data = null;
                return false;
            }

            /*if( Permissions.IsUserBlocked( senderId ) )
            {
                return;
            }

            if( m_ChannelDatas.ContainsKey( channelName ) )
            {
                return;
            }

            if( m_Manager.Client.PrivateChannels.ContainsKey( channelName ) == false )
            {
                Hashtable table = new Hashtable();
                table.Add( "type", MessageType.OpenLocalPrivateChat );
                table.Add( "receiver", senderId );
                m_Manager.Client.SendPrivateMessage( senderId, table );
            }

            CreateChannelButton( channelName, true, senderId );
            SwitchChannel( channelName );*/
        }

        ChannelData GetOrCreateChannelData( ChatChannel channel )
        {
            string key = channel.Name;
            if( m_ChannelDatas.ContainsKey( key ) == false )
            {
                m_ChannelDatas.Add( key, new ChannelData() );
            }

            m_ChannelDatas[ key ].Name = key;

            bool isPrivate = channel.MaxSubscribers == 2;
            if( isPrivate )
            {
                if( channel.TryGetCustomChannelProperty<string>( m_Manager.Client.UserId, out var privateChannelUserName ) )
                {
                    m_ChannelDatas[ key ].PrivateChannelOtherUser = privateChannelUserName;
                }
                else
                {
                    Debug.LogError( "Tried to get data for private channel but customproperties were not set!" );
                }
            }
            m_ChannelDatas[ key ].Channel = channel;
            m_ChannelDatas[ key ].IsPrivate = isPrivate;

            m_ChannelDatas[ key ].VoiceRoomName = channel.Name;

            return m_ChannelDatas[ key ];
        }

        void CreateChannelButton( ChannelData data )
        {
            if( CreateChannelButtonOverride != null )
            {
                CreateChannelButtonOverride( data );
            }
            else
            {
                data.CreateButton( ChannelButtonPrefab, ChannelListParent, ( ChannelData channelData ) => { SwitchChannel( channelData.Name, false ); } );
            }
        }

        void UpdateText()
        {
            foreach( var text in ChatlogText )
            {
                text?.SetText( m_Sb );
            }
        }

        void AddMessage( string message )
        {
            m_Sb.AppendLine();
            m_Sb.Append( message );
        }

        void AddMessage( string userId, string message )
        {
            var user = m_Manager.UserManager.GetOrCreateUser( userId );
            m_Sb.AppendLine();
            m_Sb.Append( user.Nickname );
            m_Sb.Append( ": " );
            m_Sb.Append( message );
        }

        public void BlockCurrentChannel()
        {
            if( CurrentChannel == null )
            {
                return;
            }

            m_Manager.Permissions.BlockChannel( CurrentChannel.Name );
            LeaveCurrentChannel();
        }

        public void ConfirmCurrentPrivateChat()
        {
            if( CurrentChannel == null )
            {
                return;
            }

            m_Manager.Permissions.ConfirmPrivateChat( CurrentChannel.Name );
            CreateChannelButton( CurrentChannel );
            SwitchChannel( CurrentChannel.Name, false );
        }

        public void LeaveCurrentChannel()
        {
            if( CurrentChannel != null )
            {
                /*                if( CurrentChannel.IsPrivate )
                                {
                                    ClosePrivateChat( CurrentChannel.Name );
                                }
                                else
                                {
                                }*/

                LeaveChannel( CurrentChannel.Name );
                CurrentChannel = null;
                OnLeaveCurrentChannel();
            }
        }

        public void SwitchChannel( string channelId, bool isPrivateRequestList = false )
        {
            if( CurrentChannel != null && CurrentChannel.Name == channelId )
            {
                // return;
            }

            if( string.IsNullOrEmpty( channelId ) == false && m_ChannelDatas.ContainsKey( channelId ) )// && m_Manager.Client.TryGetChannel( channelId, m_ChannelDatas[ channelId ].IsPrivate, out var channel ) )
            {

                CurrentChannel = m_ChannelDatas[ channelId ];
                PrivateMessageTarget = channelId;

                m_Sb.Clear();
                for( int i = 0; i < CurrentChannel.Channel.MessageCount; ++i )
                {
                    var table = CurrentChannel.Channel.Messages[ i ] as Hashtable;
                    var type = (MessageType)table[ "type" ];
                    string senderId = CurrentChannel.Channel.Senders[ i ];
                    switch( type )
                    {
                    case MessageType.Chat:
                        AddMessage( senderId, table[ "text" ] as string );
                        break;
                    case MessageType.InviteToGroup:
                        break;
                    case MessageType.Custom:
                        if( GetTextForCustomMessageOverride != null )
                        {
                            AddMessage( GetTextForCustomMessageOverride( CurrentChannel, senderId, table ) );
                        }
                        break;
                    default:
                        Debug.LogWarning( "Could not handle message type " + type.ToString() );
                        break;
                    }
                }
                UpdateText();
                m_Manager.UserManager.UpdateUserList( null, isPrivateRequestList );

                TopBar?.UpdateCurrent( m_Manager );
                //ChannelHeadline.text = GetCurrentChannelDisplayName( CurrentChannel );

                UpdateSelection( channelId );
                SelectChannelCallback?.Invoke( channelId );
            }
            else if( isPrivateRequestList )
            {
                CurrentChannel = null;
                PrivateMessageTarget = "";

                m_Sb.Clear();
                m_Sb.Append( "Select a user on the left to view and confirm their private chats with you." );
                UpdateText();
                m_Manager.UserManager.UpdateUserList( null, true );
                TopBar?.UpdateCurrent( m_Manager );
                UpdateSelection( null );
                SelectChannelCallback?.Invoke( "" );
            }
        }



        void UpdateSelection( string channelId )
        {
            foreach( var button in m_ChannelDatas )
            {
                button.Value.Button?.SetSelected( channelId );
            }

            UnconfirmedPrivateChatsButton?.SetSelected( channelId );
        }

        public string GetCurrentChannelName()
        {
            if( CurrentChannel == null )
            {
                return "";
            }

            return CurrentChannel.Name;
        }

        public string GetCurrentChannelDisplayName( ChatChannel channel )
        {
            if( GetChannelNameOverride != null )
            {
                return GetChannelNameOverride( channel );
            }

            if( channel == null )
            {
                return "No Channel Selected.";
            }

            if( channel.PublishSubscribers == false )
            {
                return channel.Name;
            }

            string result = "";
            foreach( string name in channel.Subscribers )
            {
                result += m_Manager.UserManager.GetOrCreateUser( name ).Nickname + ", ";
            }

            return result;
        }

        public void CreateAndJoinChannel()
        {
            string name = System.Guid.NewGuid().ToString() + "#" + ColorUtility.ToHtmlStringRGB( Color.HSVToRGB( Random.value, 1f, 1f ) );

            JoinChannel( name, true );
        }

        public void SwitchToPrivateRequestsList()
        {
            m_Manager.UserManager.UpdateUserList( "", true );
        }

        public void JoinChannel( string channelName, bool publishSubscribers, int oldMessages = 20 )
        {
            var options = new ChannelCreationOptions();
            options.PublishSubscribers = publishSubscribers;

            Debug.Log( "Join Chat Channel " + channelName );
            StartCoroutine( SubscribeWhenReadyRoutine( channelName, 0, oldMessages, options ) );
        }

        public void JoinPrivateChannel( string otherUserId )
        {
            string channelName = VoiceManager.GetVoiceRoomName( otherUserId, m_Manager.Client.UserId );

            var options = new ChannelCreationOptions();
            options.PublishSubscribers = true;
            options.MaxSubscribers = 2;

            Dictionary<string, object> properties = new Dictionary<string, object>();
            string localUserId = m_Manager.Client.UserId;
            properties.Add( otherUserId, localUserId );
            properties.Add( localUserId, otherUserId );
            options.CustomProperties = properties;

            Debug.Log( "Join Private chat with " + otherUserId );
            StartCoroutine( SubscribeWhenReadyRoutine( channelName, 0, 20, options ) );
        }

        System.Collections.IEnumerator SubscribeWhenReadyRoutine( string channelName, int lastMsgId = 0, int messagesFromHistory = -1, ChannelCreationOptions options = null )
        {
            while( m_Manager.Client == null || m_Manager.Client.State != ChatState.ConnectedToFrontEnd )
            {
                yield return null;
            }
            Debug.Log( "Subscribe To Channel: " + channelName );
            m_Manager.Client.Subscribe( channelName, lastMsgId, messagesFromHistory, options );
        }

        public void LeaveChannel( string name )
        {
            m_Manager.Client.Unsubscribe( new string[] { name } );
        }
        /*
                public void ClosePrivateChat( string channelName )
                {
                    if( m_ChannelDatas.ContainsKey( channelName ) )
                    {
                        var data = m_ChannelDatas[ channelName ];
                        if( data.Button != null )
                        {
                            RemoveChannelButton( data );
                        }
                    }
                }
                */
    }
}