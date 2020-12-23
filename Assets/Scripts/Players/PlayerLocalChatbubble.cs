using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    [RequireComponent( typeof( PlayerLocal ) )]
    public class PlayerLocalChatbubble : MonoBehaviour
    {
        public static PlayerLocalChatbubble Instance;

        Chatlog m_ChatLogPopup;

        PlayerLocal m_Player;
        Chatbubble m_CurrentBubble;

        Groups.GroupsManager GroupsManager
        {
            get
            {
                if( m_Player == null || m_Player.ChatClient == null )
                {
                    return null;
                }
                return m_Player.ChatClient.GroupsManager;
            }
        }

        private void Awake()
        {
            Instance = this;

            m_Player = GetComponent<PlayerLocal>();
            m_ChatLogPopup = GetComponentInChildren<Chatlog>();
            m_Player.GoToAndLookCallback += OnConferenceTeleport;
        }

        private void Update()
        {
            UpdateMuteIconsForPlayersInSameBubble();
        }

        void UpdateMuteIconsForPlayersInSameBubble()
        {
            if( m_CurrentBubble == null || GroupsManager.VoiceManager.Client.CurrentRoom == null )
            {
                return;
            }

            foreach( var player in m_CurrentBubble.Players )
            {
                Avatar avatar = AvatarManager.Instance.GetAvatarForPlayerUserId( player.UserId );

                if( avatar != null )
                {
                    if( avatar.CustomProperties.VoiceActorNumber == -1 
                        || GroupsManager.VoiceManager.Client.CurrentRoom.Players.ContainsKey( avatar.CustomProperties.VoiceActorNumber ) == false
                        || GroupsManager.VoiceManager.Permissions.IsUserMuted( Groups.Helper.GetAuthName( player.UserId, player.NickName ) ) )
                    {
                        avatar.ShowMuteIcon();
                    }
                }
            }
        }

        public void OnTeleport()
        {
            OnConferenceTeleport( transform, false, false );
        }

        protected void OnConferenceTeleport( Transform target, bool teleport, bool useDirection )
        {
            if( m_CurrentBubble != null )
            {
                if( m_CurrentBubble.IsInside( target.position ) == false )
                {
                    BackUI.Instance?.RemoveLast();
                    SetCurrentChatBubble( null );
                }
            }
        }

        public void SetCurrentChatBubble( Chatbubble bubble )
        {
            if( GroupsManager?.VoiceManager.Client.IsConnected == false )
            {
                Debug.LogError( "Can't join bubble. Voice is not connected to the server yet." );
                return;
            }

            if( bubble == m_CurrentBubble )
            {
                return;
            }

            byte[] joinGroups = null;
            byte[] leaveGroups = null;
            byte freePlayerPosition = 255;

            Chatbubble leavingBubble = null;
            if( m_CurrentBubble != null )
            {
                leavingBubble = m_CurrentBubble;

                if( GroupsManager != null && GroupsManager.ChannelManager != null )
                {
                    GroupsManager.ChannelManager.LeaveChannel( leavingBubble.GetVoiceRoomName() );
                }

                leaveGroups = new byte[ 1 ];
                leaveGroups[ 0 ] = leavingBubble.InterestGroup;

                leavingBubble.OnLocalPlayerLeft();
            }

            Debug.Log( "SetCurrentChatBubble: " + bubble + " . Leaving: " + m_CurrentBubble );

            if( bubble != null )
            {
                freePlayerPosition = bubble.FindClosestFreePlayerPosition( transform.position );

                if( freePlayerPosition == 0 )
                {
                    Debug.LogError( "No free position in bubble" );
                    bubble = null;
                }
            }

            if( bubble != null )
            {
                joinGroups = new byte[ 1 ];
                joinGroups[ 0 ] = bubble.InterestGroup;

                bubble.OnLocalPlayerJoined();
            }

            m_Player.ChangeInterestGroups( leaveGroups, joinGroups );

            if( bubble == null )
            {
                DoLeaveBubble( leavingBubble );
            }
            else
            {
                DoJoinBubble( bubble, freePlayerPosition );
            }

            m_CurrentBubble = bubble;
        }

        void DoJoinBubble( Chatbubble bubble, byte freePlayerPosition )
        {
            string bubbleRoom = bubble.GetVoiceRoomName();
            m_Player.ChatClient.GroupsManager.ChannelManager.JoinChannel( bubbleRoom, true );
            m_Player.ChatClient.GroupsManager.VoiceManager.JoinRoom( bubbleRoom );

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add( Chatbubble.CurrentChatBubblePositionProperty, freePlayerPosition );
            properties.Add( Chatbubble.CurrentChatBubbleProperty, bubble.InterestGroup );

            m_Player.CustomProperties.SetCustomProperties( properties );

            m_Player.EnableFullBodySerialization( bubble.InterestGroup );

            GotoBubble( bubble, freePlayerPosition );
        }

        void DoLeaveBubble( Chatbubble leavingBubble )
        {
            if( leavingBubble != null && GroupsManager.VoiceManager.Client.CurrentRoom?.Name == leavingBubble.GetVoiceRoomName() )
            {
                GroupsManager.VoiceManager.LeaveRoom();
            }

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add( Chatbubble.CurrentChatBubblePositionProperty, (byte)0 );
            properties.Add( Chatbubble.CurrentChatBubbleProperty, (byte)0 );

            m_Player.CustomProperties.SetCustomProperties( properties );

            m_Player.DisableFullBodySerialization();
        }

        public void ConnectToCurrentBubbleVoiceRoom()
        {
            if( m_CurrentBubble != null )
            {
                GroupsManager.VoiceManager.JoinRoom( m_CurrentBubble.GetVoiceRoomName() );
            }
        }

        public Chatbubble GetCurrentChatBubble()
        {
            return m_CurrentBubble;
        }

        void GotoBubble( Chatbubble bubble, byte position )
        {
            if( bubble == null )
            {
                return;
            }

            Transform targetTransform = bubble.GetTargetTransformForPosition( position );
            PlayerLocal.Instance.GoToAndLook( targetTransform, false, true );
        }
    }
}