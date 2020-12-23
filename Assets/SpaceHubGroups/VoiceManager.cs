
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;
using Photon.Voice;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace SpaceHub.Groups
{
    public class VoiceManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
    {

        public const string VoicePropertyPrefix = "voice_";

        GroupsManager m_Manager;
        public VoiceControl Controls { get; private set; }

        public LoadBalancingTransport Client { get { return VoiceConnection.Client; } }
        public ChannelPermissions Permissions { get { return m_Manager.Permissions; } }

        public VoiceConnection VoiceConnection { get; private set; }
        string m_JoinRoomNameOnLeftRoom;

        string m_CurrentRoom = "";

        private void Awake()
        {
            m_Manager = GetComponent<GroupsManager>();
            Controls = GetComponentInChildren<VoiceControl>();
        }

        private void Start()
        {
            StartCoroutine( SetChatIdWhenReady() );
        }

        private void OnDestroy()
        {
            Client.RemoveCallbackTarget( this );
        }

        System.Collections.IEnumerator SetChatIdWhenReady()
        {
            while( m_Manager.Client == null || Client == null )
            {
                yield return null;
            }
            var playerprops = new Hashtable();
            playerprops.Add( "chat_senderId", m_Manager.Client.UserId );
            Client.LocalPlayer.SetCustomProperties( playerprops );
        }

        public void SetConnection( VoiceConnection connection )
        {
            VoiceConnection = connection;
            VoiceConnection.SpeakerLinked += OnSpeakerLinked;

            Client.AddCallbackTarget( this );

            
        }

        void OnSpeakerLinked( Speaker speaker )
        {
            Debug.Log( "Speaker linked", speaker );
            var mute = speaker.gameObject.AddComponent<VoiceMute>();
            mute.m_Permissions = Permissions;

            string senderId = "";

            if( string.IsNullOrEmpty( speaker.Actor.UserId ) )
            {
                speaker.Actor.CustomProperties.TryGetValue( "chat_senderId", out var senderObject );
                if( senderObject != null )
                {
                    senderId = (string)senderObject;
                }
            }
            else
            {
                senderId = Helper.GetAuthName( speaker.Actor.UserId, speaker.Actor.NickName );
            }
            mute.ChatId = senderId as string;
            mute.UpdateMuteStatus();
        }

        public void JoinRoom( string roomName )
        {
            m_JoinRoomNameOnLeftRoom = "";
            Debug.Log( "Join voice room: " + roomName );

            if( Client.State == ClientState.ConnectedToMasterServer )
            {
                RaiseJoinRoomEvent( roomName );
            }
            else
            {
                m_JoinRoomNameOnLeftRoom = roomName;

                if( Client.State == ClientState.Joined )
                {
                    Client.OpLeaveRoom( false );
                }
            }
        }

        void RaiseJoinRoomEvent( string roomName )
        {
            Debug.Log( "RaiseJoinRoomEvent to " + roomName );
            EnterRoomParams enterRoomParams = new EnterRoomParams();
            enterRoomParams.RoomName = roomName;
            enterRoomParams.RoomOptions = new RoomOptions();
            enterRoomParams.RoomOptions.MaxPlayers = 200;
            enterRoomParams.Lobby = TypedLobby.Default;

            Client.OpJoinOrCreateRoom( enterRoomParams );

            if( VoiceConnection.PrimaryRecorder.RequiresRestart )
            {
                VoiceConnection.PrimaryRecorder.RestartRecording();
            }

            m_Manager.ChannelManager.UpdateCallIcons( roomName );
        }

        public void OnChannelPropertiesChanged( string channel, string userId, Dictionary<object, object> properties )
        {
            m_Manager.ChannelManager.UpdateCallIcons( m_CurrentRoom );
        }


        public static string GetVoiceRoomName( string userA, string userB )
        {
            if( string.Compare( userA, userB ) < 0 )
            {
                return userA + userB;
            }

            return userB + userA;
        }

        public void LeaveRoom()
        {
            if( Client.InRoom == true )
            {
                Client.OpLeaveRoom( false );
            }
        }

        public void OnConnectedToMaster()
        {
            if( string.IsNullOrEmpty( m_JoinRoomNameOnLeftRoom ) == false )
            {
                RaiseJoinRoomEvent( m_JoinRoomNameOnLeftRoom );
                m_JoinRoomNameOnLeftRoom = "";
            }
        }

        public void OnJoinedRoom()
        {
            m_CurrentRoom = Client.CurrentRoom?.Name;
            m_Manager.ChannelManager.UpdateCallIcons( m_CurrentRoom );
            SetInVoiceRoom( m_CurrentRoom, true );
        }

        public void OnLeftRoom()
        {
            m_Manager.ChannelManager.UpdateCallIcons( "" );
            SetInVoiceRoom( m_CurrentRoom, false );
            m_CurrentRoom = "";
        }

        void SetInVoiceRoom( string roomName, bool value )
        {
            var table = new Dictionary<string, object>();
            table.Add( VoicePropertyPrefix + m_Manager.Client.UserId, value );
            m_Manager.Client.SetCustomChannelProperties( roomName, table );

            Debug.Log( "SetChannelProperties Room:" + roomName + "\nSet Voice for " + m_Manager.Client.UserId + " to " + value.ToString() );
        }

        public void OnConnected() { }
        public void OnDisconnected( DisconnectCause cause ) { }
        public void OnRegionListReceived( RegionHandler regionHandler ) { }
        public void OnCustomAuthenticationResponse( Dictionary<string, object> data ) { }
        public void OnCustomAuthenticationFailed( string debugMessage ) { }

        public void OnFriendListUpdate( List<FriendInfo> friendList ) { }
        public void OnCreatedRoom() { }
        public void OnCreateRoomFailed( short returnCode, string message ) { }

        public void OnJoinRoomFailed( short returnCode, string message ) { }
        public void OnJoinRandomFailed( short returnCode, string message ) { }


    }
}
