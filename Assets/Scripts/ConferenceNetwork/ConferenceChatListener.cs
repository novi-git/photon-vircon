using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Chat;
using PhotonTable = ExitGames.Client.Photon.Hashtable;

namespace SpaceHub.Conference
{

    public class ConferenceChatListener : MonoBehaviour, IChatClientListener, Photon.Realtime.IMatchmakingCallbacks
    {

        public enum CustomMessageType
        {
            none,
            emote,
            signal,
        }
        public static ConferenceChatListener Instance;

        Groups.ChatConnector m_Chat;
        Groups.GroupsManager m_Groups;
        string m_CurrentScene;

        void Awake()
        {
            Instance = this;
        }

        IEnumerator Start()
        {
            m_Chat = GetComponent<Groups.ChatConnector>();
            m_Chat.Listeners.Add( this );
            ConferenceRoomManager.Instance.SceneChangedCallback += OnSceneChanged;

            string region = PlayerPrefs.GetString( "PhotonRegion", "asia" );
            m_Chat.AppVersion = VersionData.AppVersion;

            m_Groups = ViewModeManager.Instance.CurrentViewMode.XRSystem.gameObject.GetComponentInChildren<Groups.GroupsManager>();

            m_Chat.SetGroupsManager( m_Groups );
            m_Groups.VoiceManager.SetConnection( PlayerLocal.Instance.VoiceConnection );

            m_Groups.VoiceManager.Client.AddCallbackTarget( this );
            m_Groups.VoiceManager.Controls.HideControllsOverride = true;
            m_Groups.VoiceManager.Controls.ConfirmLeaveCall = ConfirmLeaveCallPopup;

            m_Groups.ChannelManager.LeaveCurrentChannelCallback += OnCurrentChannelLeft;
            m_Groups.ChannelManager.GetChannelNameOverride = GetChannelName;
            m_Groups.ChannelManager.SelectChannelCallback += OnSelectChannel;

            m_Groups.ChannelManager.ReceiveCustomMessageCallback += OnReceiveCustomMessage;
            m_Groups.ChannelManager.GetTextForCustomMessageOverride = GetTextForCustomMessage;

            m_Groups.ReceiveCustomPrivateMessageCallback += OnReceiveCustomPrivateMessage;

            m_Groups.MinimizerChatLog?.SetEnabledAndVisible( false, false );
            while( PlayerLocal.Instance.Client.IsConnectedAndReady == false )
            {
                yield return null;
            }

            m_Chat.AppId = ConferenceServerSettings.Instance.GetChatAppId();
            m_Chat.AppVersion = ConferenceServerSettings.Instance.GetAppVersion();

            m_Chat.Connect( PlayerLocal.Instance.Client.UserId, CustomizationData.Instance.Nickname, region );

            m_Groups.ChannelManager.JoinChannel( SignalManager.SignalChannelName, false, 0 );
        }

        private void OnDestroy()
        {
            if( m_Chat != null && m_Chat.Listeners != null && m_Chat.Listeners.Contains( this ) )
            {
                m_Chat.Listeners.Remove( this );
                m_Groups.ChannelManager.LeaveCurrentChannelCallback -= OnCurrentChannelLeft;
            }
        }

        private void Update()
        {
            m_Groups.ChatInput.ChatEnabledOverride = MessagePopupMessage.IsInputFieldInUse() == false;
        }

        void ConfirmLeaveCallPopup( UnityAction onSuccess )
        {
            if( PlayerLocalChatbubble.Instance.GetCurrentChatBubble() != null && BackUI.Instance != null )
            {
                MessagePopup.ShowConfirm( "Leaving the call means leaving the Chatbubble. Are you sure?", "Leave", "Stay", BackUI.Instance.OnClick, null );
            }
            else
            {
                onSuccess?.Invoke();
            }
        }

        void OnCurrentChannelLeft()
        {
            m_Groups.ChannelManager.SwitchChannel( m_CurrentScene );
        }

        void OnSceneChanged( string oldRoom, string newRoom )
        {
            if( m_Chat.m_Client == null || m_Chat.m_Client.State != ChatState.ConnectedToFrontEnd )
            {
                return;
            }

            if( string.IsNullOrEmpty( m_CurrentScene ) == false )
            {
                m_Groups.ChannelManager.LeaveChannel( m_CurrentScene );
            }

            m_Groups.ChannelManager.JoinChannel( newRoom, false, 0 );

            Debug.Log( "Chat Channel Changed from " + m_CurrentScene + " to " + newRoom );
            m_CurrentScene = newRoom;
        }

        void OnSelectChannel( string channelId )
        {
            if( channelId == SignalManager.SignalChannelName )
            {
                m_Groups.ChannelManager.SwitchChannel( m_CurrentScene );
                return;
            }

            bool channelIsRoomChannel = ChannelIsSceneChannel( channelId );
            bool showChatlog = !channelIsRoomChannel || ConferenceSceneSettings.Instance.ChatLogEnabledForRoom;
            bool showChatInput = ( !channelIsRoomChannel || ConferenceSceneSettings.Instance.ChatInputEnabled ) && ViewModeManager.SelectedViewMode == ViewModeManager.ViewMode.ThirdPerson;
            m_Groups.MinimizerChatLog?.SetEnabled( showChatlog );
            m_Groups.MinimizerChatInput?.SetEnabled( showChatInput );

            UpdateVoiceControllsActive();
        }

        bool ChannelIsSceneChannel( string channelId )
        {
            return channelId == m_CurrentScene;
        }

        void UpdateVoiceControllsActive()
        {
            bool isSceneChannel = ChannelIsSceneChannel( m_Groups.ChannelManager.CurrentChannel?.Name );
            bool isVoiceCallActive = m_Groups.VoiceManager.Client?.CurrentRoom != null;

            m_Groups.VoiceManager.Controls.HideControllsOverride = isSceneChannel && !isVoiceCallActive;
        }

        void OnReceiveCustomPrivateMessage( string senderId, PhotonTable table )
        {
            var type = GetCustomMessageType( table );
            switch( type )
            {
            case CustomMessageType.signal:
                if( senderId != m_Chat.m_Client.UserId )
                {
                    SignalManager.Instance.OnReceiveSignal( table, senderId );
                }
                break;
            }
        }

        void OnReceiveCustomMessage( Groups.ChannelData channelData, string senderId, PhotonTable table )
        {
            var type = GetCustomMessageType( table );
            switch( type )
            {
            case CustomMessageType.emote:
                var avatar = AvatarManager.Instance.GetAvatarForPlayerUserId( Groups.Helper.GetIdFromSenderId( senderId ) );
                if( avatar == null || avatar.Animator == null )
                {
                    break;
                }

                var id = (byte)table[ "animId" ];
                avatar.Animator.PlayAnimation( id );
                break;
            case CustomMessageType.signal:
                SignalManager.Instance.OnReceiveSignal( table, senderId );
                break;
            }
        }

        CustomMessageType GetCustomMessageType( PhotonTable table )
        {
            if( table.ContainsKey( "customType" ) == false )
            {
                Debug.LogWarning( "Custom message did not contain custom message type" );
                return CustomMessageType.none;
            }

            return (CustomMessageType)table[ "customType" ];
        }

        public string GetTextForCustomMessage( Groups.ChannelData channelData, string senderId, PhotonTable table )
        {
            var type = GetCustomMessageType( table );
            switch( type )
            {
            case CustomMessageType.emote:
                var emotes = CustomizationData.Instance.GetAllEmotes();
                var id = (byte)table[ "animId" ];
                try
                {
                    var emote = emotes[ id ];
                    return "<color=#a7c9f2><i>" + Groups.Helper.GetNicknameFromSenderid( senderId ) + " " + emote.ChatSentence + "</i></color>";
                }
                catch
                {
                    Debug.LogWarning( "Could not get emote for id " + id );
                    return null;
                }
            }

            return null;
        }



        public void SendEmoteMessageToCurrentChannel( byte animId, byte emojiId )
        {
            var table = new PhotonTable();
            table.Add( "animId", animId );
            table.Add( "emojiId", emojiId );
            table.Add( "customType", CustomMessageType.emote );


            m_Groups.ChannelManager.SendCustomMessageToCurrentChannel( table );

            PlayerLocal.Instance.AvatarAnimator?.PlayAnimation( animId );
        }

        string GetChannelName( ChatChannel channel )
        {
            if( channel == null )
            {
                return "No Channel Selected.";
            }
            if( channel.Name == m_CurrentScene )
            {
                return ConferenceSceneSettings.Instance.ChatRoomChannelDisplayName + ":";
            }

            var chatbubble = PlayerLocalChatbubble.Instance.GetCurrentChatBubble();
            if( chatbubble != null &&
                 channel.Name == chatbubble.GetVoiceRoomName() )
            {
                return "Chat Bubble:";
            }

            if( channel.MaxSubscribers == 2 )
            {
                return "(private) " + channel.Name;
            }
            string result = "";
            foreach( string name in channel.Subscribers )
            {
                result += Groups.Helper.GetNicknameFromSenderid( name ) + ", ";
            }
            return result;
        }

        public void OnDisconnected() { }

        public void OnConnected()
        {
            OnSceneChanged( "", ConferenceRoomManager.Instance.CurrentRoomName );
        }

        public void DebugReturn( ExitGames.Client.Photon.DebugLevel level, string message ) { }
        public void OnChatStateChange( ChatState state ) { }
        public void OnGetMessages( string channelName, string[] senders, object[] messages ) { }
        public void OnPrivateMessage( string sender, object message, string channelName ) { }
        public void OnSubscribed( string[] channels, bool[] results ) { }
        public void OnUnsubscribed( string[] channels ) { }
        public void OnStatusUpdate( string user, int status, bool gotMessage, object message ) { }
        public void OnUserSubscribed( string channel, string user ) { }
        public void OnUserUnsubscribed( string channel, string user ) { }


        // ########### Photon Voice Callbacks

        public void OnJoinedRoom()
        {
            UpdateVoiceControllsActive();
        }
        public void OnLeftRoom()
        {
            UpdateVoiceControllsActive();
        }

        public void OnFriendListUpdate( List<Photon.Realtime.FriendInfo> friendList ) { }
        public void OnCreatedRoom() { }
        public void OnCreateRoomFailed( short returnCode, string message ) { }

        public void OnJoinRoomFailed( short returnCode, string message ) { }
        public void OnJoinRandomFailed( short returnCode, string message ) { }

        public void OnChannelPropertiesChanged( string channel, string userId, Dictionary<object, object> properties ) { }
        public void OnUserPropertiesChanged( string channel, string targetUserId, string senderUserId, Dictionary<object, object> properties ) { }
        public void OnErrorInfo( string channel, string error, object data ) { }
    }
}
