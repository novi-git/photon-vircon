using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using UnityEngine.EventSystems;

namespace SpaceHub.Conference
{
    public class AvatarInfoPopup : PopupBase
    {
        public static AvatarInfoPopup Instance;

        public TextMeshProUGUI NameText;
        public TextMeshProUGUI CompanyText;
        public TextMeshProUGUI PrivateCallButtonText;
        public TextMeshProUGUI ProfileFirstLastName;

        public Button PrivateChatButton;
        public Button InviteToGroupButton;

        public Button MuteButton;
        public Button UnmuteButton;

        public Button BlockButton;
        public Button UnblockButton;

        public GameObject LinkedInButton;
        public GameObject FacebookButton;
        public GameObject TwitterButton;
        public GameObject EmailButton;

        public Groups.GroupsManager GroupsManager;

        Avatar m_Avatar;

        IEnumerator Start()
        {
            Instance = this;


            Close();
            PlayerLocal.Instance.Connector.PlayerPropertiesUpdateCallback += OnPlayerPropertiesUpdate;

            while( PlayerLocal.Instance.ChatClient.GroupsManager == null )
            {
                yield return null;
            }
            GroupsManager = PlayerLocal.Instance.ChatClient.GroupsManager;

            GroupsManager.UserManager.UserClickedCallback += OnChatUserClicked;
            GroupsManager.ChannelManager.SelectChannelCallback += OnChannelSelected;
            GroupsManager.Permissions.BlockedChannelsChangedCallback += UpdateVisuals;
            GroupsManager.UserManager.UserListUpdatedCallback += UpdateInviteToGroupButton;
            //PlayerLocal.Instance.OnTeleportCallback += Close;
        }

        void OnChannelSelected( string channelName )
        {
            UpdateInviteToGroupButton();
        }

        void OnChatUserClicked( string senderId )
        {
            string userId = Groups.Helper.GetIdFromSenderId( senderId );
            var avatar = AvatarManager.Instance.GetAvatarForPlayerUserId( userId );

            if( avatar != null )
            {
                Open( avatar );
            }
        }

        void UpdateVisuals()
        {
            if( m_Avatar == null )
            {
                return;
            }
            NameText.text = m_Avatar.CustomProperties.NickName;
            CompanyText.text = m_Avatar.CustomProperties.CompanyName;

            switch( m_Avatar.CustomProperties.CallStatus )
            {
            case ConferenceCustomProperties.CallStatusType.Available:
                PrivateCallButtonText.text = "Send Call Request";
                break;
            default:
                PrivateCallButtonText.text = "Cannot Call (" + m_Avatar.CustomProperties.CallStatus + ")";
                break;
            }

            bool blocked = false;
            bool muted = false;
            if( GroupsManager != null )
            {
                blocked = GroupsManager.Permissions.IsUserBlocked( GetChatAuthName() );
                muted = GroupsManager.Permissions.IsUserMuted( GetChatAuthName() );
            }

            if( PrivateChatButton != null )
            {
                PrivateChatButton.interactable = m_Avatar.CustomProperties.CallStatus == ConferenceCustomProperties.CallStatusType.Available;
                PrivateChatButton.gameObject.SetActive( !blocked );
            }
            if( MuteButton != null )
            {
                MuteButton.gameObject.SetActive( !muted );
            }
            if( UnmuteButton != null )
            {
                UnmuteButton.gameObject.SetActive( muted );
            }

            if( BlockButton != null )
            {
                BlockButton.gameObject.SetActive( !blocked );
            }
            if( UnblockButton != null )
            {
                UnblockButton.gameObject.SetActive( blocked );
            }

            UpdateProfileName();

            UpdateProfileButton( LinkedInButton, "LinkedIn" );
            UpdateProfileButton( FacebookButton, "Facebook" );
            UpdateProfileButton( TwitterButton, "Twitter" );
            UpdateProfileButton( EmailButton, "PublicEmail" );

            UpdateInviteToGroupButton();
        }

        void UpdateInviteToGroupButton()
        {
            if( IsOpen() && InviteToGroupButton != null && m_Avatar != null )
            {
                string authName = Groups.Helper.GetAuthName( m_Avatar.Player.UserId, m_Avatar.CustomProperties.NickName );

                var data = GroupsManager.ChannelManager.CurrentChannel;

                bool canInvite = data != null
                        && data.Name != ConferenceRoomManager.Instance.CurrentRoomName;

                var bubble = PlayerLocalChatbubble.Instance.GetCurrentChatBubble();
                canInvite &= ( bubble == null || bubble.GetVoiceRoomName() != data.Name );

                canInvite &= data != null && data.Channel != null && data.IsPrivate == false && data.Channel.Subscribers != null && data.Channel.Subscribers.Contains( authName ) == false;

                InviteToGroupButton.interactable = canInvite;
            }
        }

        void UpdateProfileName()
        {
            List<string> nameStrings = new List<string>();
            nameStrings.Add( m_Avatar.Player.GetStringProperty( "FirstName" ) );
            nameStrings.Add( m_Avatar.Player.GetStringProperty( "LastName" ) );
            nameStrings.RemoveAll( item => item == "" );

            ProfileFirstLastName.gameObject.SetActive( nameStrings.Count > 0 );
            if( nameStrings.Count > 0 )
            {
                ProfileFirstLastName.text = string.Join( " ", nameStrings );
            }
        }

        void UpdateProfileButton( GameObject button, string customProperty )
        {
            button.SetActive( m_Avatar.Player.GetStringProperty( customProperty ) != "" );
        }

        public void StartPrivateCall()
        {
            if( m_Avatar.CustomProperties.CallStatus == ConferenceCustomProperties.CallStatusType.Available )
            {
                Close();

                PlayerLocal.Instance.PrivateCalls.SendCallRequestTo( m_Avatar.Player );
            }

            EventSystem.current.SetSelectedGameObject( null );
        }

        public void Open( Avatar avatar )
        {
            m_Avatar = avatar;

            UpdateVisuals();
            DoOpen();
        }

        public void Close()
        {
            DoClose();
        }

        void OnPlayerPropertiesUpdate( Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps )
        {
            if( IsOpen() == true &&
                m_Avatar != null &&
                m_Avatar.Player == targetPlayer )
            {
                UpdateVisuals();
            }
        }

        void OpenUrlForProperty( string baseUrl, string property )
        {
            Helpers.OpenURL( baseUrl + m_Avatar.Player.GetStringProperty( property ) );
        }

        public void OnLinkedInButtonClicked()
        {
            OpenUrlForProperty( "https://de.linkedin.com/in/", "LinkedIn" );
        }

        public void OnFacebookButtonClicked()
        {
            OpenUrlForProperty( "https://www.facebook.com/", "Facebook" );
        }

        public void OnTwitterButtonClicked()
        {
            OpenUrlForProperty( "https://twitter.com/", "Twitter" );
        }

        public void OnEmailButtonClicked()
        {
            OpenUrlForProperty( "mailto:", "PublicEmail" );
        }

        public void OnInviteToCurrentChannelClicked()
        {
            if( GroupsManager != null )
            {
                GroupsManager.UserManager.InviteToCurrentChannel( GetChatAuthName() );
                MessagePopup.Show( "Send Group invite to " + m_Avatar.CustomProperties.NickName );
            }
        }

        string GetChatAuthName()
        {
            return Groups.Helper.GetAuthName( m_Avatar.Player.UserId, m_Avatar.CustomProperties.NickName );
        }

        public void BlockClicked()
        {
            if( GroupsManager != null )
            {
                MessagePopup.ShowConfirm( "Are you sure you want to block this user? You will not be able to interact with them anymore.", "Yes, block.", "No", () =>
                 {
                     GroupsManager.Permissions.BlockUser( GetChatAuthName() );
                     AvatarManager.Instance.OnPlayerLeftRoom( m_Avatar.Player );
                     UpdateVisuals();
                     Close();
                 },
                 () =>
                 {
                 } );
            }
        }

        public void UnblockClicked()
        {
            if( GroupsManager != null )
            {
                GroupsManager.Permissions.UnblockUser( GetChatAuthName() );
                AvatarManager.Instance.OnPlayerEnteredRoom( m_Avatar.Player );
                UpdateVisuals();
            }
        }

        public void MuteClicked()
        {
            if( GroupsManager != null )
            {
                GroupsManager.Permissions.MuteUser( GetChatAuthName() );
                UpdateVisuals();
            }
        }
        public void UnmuteClicked()
        {
            if( GroupsManager != null )
            {
                GroupsManager.Permissions.UnmuteUser( GetChatAuthName() );
                UpdateVisuals();
            }
        }

        public void OnReportClicked()
        {
            MessagePopup.ShowConfirmWithInput( "Are you sure you want to report this user?", "Please enter a reason here...", "Send Report", "Cancel",
                ( string value ) =>
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add( "TargetUserId", m_Avatar.Player.UserId );
                    parameters.Add( "Message", value );
                    PlayerLocal.Instance.Client.OpWebRpc( "report", parameters );
                    MessagePopup.Show( "Report Sent!", LogType.Log );
                }
            , null );
        }

        public void OpenAdminViewClicked()
        {
            ModUserPopup.Instance.Show( m_Avatar.Player.UserId );
        }

        public void OnStartPrivateChatClicked()
        {
            if( GroupsManager != null )
            {
                var authName = Groups.Helper.GetAuthName( m_Avatar.Player.UserId, m_Avatar.CustomProperties.NickName );
                //var channelName = GroupsManager.Client.GetPrivateChannelNameByUser( authName );
                GroupsManager.ChannelManager.SendOpenLocalPrivateChat( authName );

                //GroupsManager.ChannelManager.TryGetOrCreatePrivateChannelData( authName );
                //GroupsManager.UserManager.InviteToGroupChannel( authName, channelName, true );
                //MessagePopup.Show( "Send Private Chat invite to " + m_Avatar.CustomProperties.NickName );
            }
        }
    }
}