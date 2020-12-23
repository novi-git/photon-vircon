using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class ConferenceVoiceConnection : VoiceConnection, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks
    {
        public static ConferenceVoiceConnection Instance;

        string m_LastJoinRoomName;
        string m_JoinRoomNameOnLeftRoom;

        public UnityAction<Player, Room> PlayerLeftRoomCallback;
        public UnityAction<string> LeftRoomCallback;

        PlayerBase m_Player;

        new void Awake()
        {
            Instance = this;
            SpeakerFactory = VoiceSpeakerFactory;

            base.Awake();
        }

        public void ConnectAndSetupForPlayer( PlayerBase player )
        {
            Connect();

            m_Player = player;

            PrimaryRecorder.UserData = m_Player.Player.ActorNumber;

            if( PrimaryRecorder.RequiresRestart )
            {
                PrimaryRecorder.RestartRecording();
            }
        }

        bool Connect()
        {
            if( this.Client.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected )
            {
                if( this.Logger.IsWarningEnabled )
                {
                    this.Logger.LogWarning( "ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: {0}", this.Client.LoadBalancingPeer.PeerState );
                }
                return false;
            }
            if( AppQuits )
            {
                if( this.Logger.IsWarningEnabled )
                {
                    this.Logger.LogWarning( "Can't connect: Application is closing. Unity called OnApplicationQuit()." );
                }
                return false;
            }
            if( this.Settings == null )
            {
                if( this.Logger.IsErrorEnabled )
                {
                    this.Logger.LogError( "Settings are null" );
                }
                return false;
            }
            if( string.IsNullOrEmpty( this.Settings.AppIdVoice ) && string.IsNullOrEmpty( this.Settings.Server ) )
            {
                if( this.Logger.IsErrorEnabled )
                {
                    this.Logger.LogError( "Provide an AppId or a Server address in Settings to be able to connect" );
                }
                return false;
            }

            this.Client.LoadBalancingPeer.TransportProtocol = this.Settings.Protocol;
            if( this.Client.LoadBalancingPeer.TransportProtocol != ConnectionProtocol.Udp && this.Logger.IsWarningEnabled )
            {
                this.Logger.LogWarning( "Requested protocol could be not fully supported: {0}. Only UDP is recommended and tested.", this.Settings.Protocol );
            }

            this.Client.EnableLobbyStatistics = this.Settings.EnableLobbyStatistics;

            this.Client.LoadBalancingPeer.DebugOut = this.Settings.NetworkLogging;

            if( this.Settings.IsMasterServerAddress )
            {
                this.Client.LoadBalancingPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV16;

                if( string.IsNullOrEmpty( this.Client.UserId ) )
                {
                    this.Client.UserId = Guid.NewGuid().ToString();
                }

                this.Client.IsUsingNameServer = false;
                this.Client.MasterServerAddress = this.Settings.Port == 0 ? this.Settings.Server : string.Format( "{0}:{1}", this.Settings.Server, this.Settings.Port );

                return this.Client.ConnectToMasterServer();
            }

            this.Client.AppId = ConferenceServerSettings.Instance.GetVoiceAppId();
            this.Client.AppVersion = ConferenceServerSettings.Instance.GetAppVersion();

            if( !this.Settings.IsDefaultNameServer )
            {
                this.Client.NameServerHost = this.Settings.Server;
            }

            if( this.Settings.IsBestRegion )
            {
                return this.Client.ConnectToNameServer();
            }

            return this.Client.ConnectToRegionMaster( this.Settings.FixedRegion );
        }

        private void OnEnable()
        {
            Client.AddCallbackTarget( this );
        }

        private new void OnDisable()
        {
            Client.RemoveCallbackTarget( this );

            base.OnDisable();
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

            if( PrimaryRecorder.RequiresRestart )
            {
                PrimaryRecorder.RestartRecording();
            }
        }

        public void LeaveRoom()
        {
            if( Client.InRoom == true )
            {
                Client.OpLeaveRoom( false );
            }
        }

        protected Speaker VoiceSpeakerFactory( int playerId, byte voiceId, object userData )
        {
            var bubble = PlayerLocalChatbubble.Instance.GetCurrentChatBubble();
            if( bubble != null )
            {

                Speaker speaker;
                int findPlayerId = playerId;

                if( userData is int )
                {
                    findPlayerId = (int)userData;
                    //Debug.Log( "PlayerId: " + playerId + ". UserData Player Id: " + findPlayerId );
                }

                Avatar avatar = AvatarManager.Instance.GetAvatarForActorNumber( findPlayerId );

                if( avatar != null )
                {
                    speaker = avatar.GetComponentInChildren<Speaker>();
                    //speaker.Actor = avatar.Player; // NOTE: commented out by Hamza, this is not needed as VoiceSpeaker.LinkSpeaker will set the Actor based on the playerId which is the ActorNumber
                }
                else
                {
                    speaker = new GameObject( "Default Speaker for " + findPlayerId ).AddComponent<Speaker>();
                    Debug.LogWarning( "No avatar found for playerId " + findPlayerId + ". Creating default speaker. Local Player Id: " + PlayerLocal.Instance.Player.ActorNumber );
                }

                return speaker;
            }
            else
            {
                var speaker = this.SimpleSpeakerFactory( playerId, voiceId, userData );
                speaker.transform.SetParent( PlayerLocal.Instance.transform.root );
                return speaker;
            }

        }

        public void OnCreatedRoom()
        {

        }

        public void OnCreateRoomFailed( short returnCode, string message )
        {

        }

        public void OnFriendListUpdate( List<FriendInfo> friendList )
        {

        }

        public void OnJoinedRoom()
        {
            m_LastJoinRoomName = Client.CurrentRoom.Name;
            Debug.Log( "Joined voice room: " + m_LastJoinRoomName );

            if( m_Player.CustomProperties.VoiceActorNumber != Client.LocalPlayer.ActorNumber )
            {
                m_Player.CustomProperties.VoiceActorNumber = Client.LocalPlayer.ActorNumber;
            }
        }

        public void OnJoinRandomFailed( short returnCode, string message )
        {

        }

        public void OnJoinRoomFailed( short returnCode, string message )
        {
            Debug.LogErrorFormat( "OnJoinRoomFailed roomName={0} errorCode={1} errorMessage={2}", m_LastJoinRoomName, returnCode, message );
        }

        public void OnLeftRoom()
        {
            Debug.Log( "Left voice room: " + m_LastJoinRoomName );
            LeftRoomCallback?.Invoke( m_LastJoinRoomName );
        }

        public void OnConnected()
        {

        }

        public void OnConnectedToMaster()
        {
            if( string.IsNullOrEmpty( m_JoinRoomNameOnLeftRoom ) == false )
            {
                RaiseJoinRoomEvent( m_JoinRoomNameOnLeftRoom );
                m_JoinRoomNameOnLeftRoom = "";
            }
        }

        public void OnDisconnected( DisconnectCause cause )
        {
            DebugPanel.DisconnectCauseVoice = cause.ToString();
            if( cause == DisconnectCause.None || cause == DisconnectCause.DisconnectByClientLogic )
            {
                return;
            }
            Debug.LogErrorFormat( "OnDisconnected cause={0}", cause );
        }

        public void OnRegionListReceived( RegionHandler regionHandler )
        {

        }

        public void OnCustomAuthenticationResponse( Dictionary<string, object> data )
        {

        }

        public void OnCustomAuthenticationFailed( string debugMessage )
        {

        }

        public void OnPlayerEnteredRoom( Player newPlayer )
        {

        }

        public void OnPlayerLeftRoom( Player otherPlayer )
        {
            PlayerLeftRoomCallback?.Invoke( otherPlayer, Client.CurrentRoom );
        }

        public void OnRoomPropertiesUpdate( ExitGames.Client.Photon.Hashtable propertiesThatChanged )
        {

        }

        public void OnPlayerPropertiesUpdate( Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps )
        {

        }

        public void OnMasterClientSwitched( Player newMasterClient )
        {

        }
    }
}