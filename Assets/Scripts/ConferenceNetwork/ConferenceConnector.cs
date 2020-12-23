using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceConnector : MonoBehaviour, IInRoomCallbacks, IOnEventCallback, IMatchmakingCallbacks, IConnectionCallbacks, IWebRpcCallback
        {
            public bool AutoConnectOnStart = true;
            public bool ReconnectOnDisconnect = true;
           
            public ConferenceNetwork Network { get; private set; }

            public UnityAction<Player> MasterClientSwitchedCallback;
            public UnityAction<Player> PlayerEnteredRoomCallback;
            public UnityAction<Player> PlayerLeftRoomCallback;

            public UnityAction<Player, ExitGames.Client.Photon.Hashtable> PlayerPropertiesUpdateCallback;
            public UnityAction<ExitGames.Client.Photon.Hashtable> RoomPropertiesUpdateCallback;

            public UnityAction<EventData> EventCallback;

            public UnityAction<List<FriendInfo>> FriendListUpdateCallback;

            public UnityAction CreatedRoomCallback;
            public UnityAction JoinedRoomCallback;
            public UnityAction LeftRoomCallback;

            public UnityAction<short, string> CreateRoomFailedCallback;
            public UnityAction<short, string> JoinRoomFailedCallback;
            public UnityAction<short, string> JoinRandomFailedCallback;

            public UnityAction ConnectedCallback;
            public UnityAction ConnectedToMasterCallback;
            public UnityAction<DisconnectCause> DisconnectedCallback;

            public UnityAction<OperationResponse> WebRpcCallback;
            public bool IsTestPlayer;
            public bool IsAdmin;

            string m_JoinRoomNameOnLeftRoom;
            bool m_ReconnectOnDisconnect = false;

            public bool IsMessageQueueRunning
            {
                get
                {
                    return Network.IsMessageQueueRunning;
                }
                set
                {
                    Network.IsMessageQueueRunning = value;
                }
            }

            private void OnEnable()
            {
                Network = new ConferenceNetwork( IsTestPlayer );
            }

            void Start()
            {
                if( AutoConnectOnStart == true )
                {
                    Network.Connect();
                }

                Network.Client.AddCallbackTarget( this );
                var handler = GetComponent<ConnectionHandler>();
                if ( handler )
                {
                    handler.Client = Network.Client;
                    handler.StartFallbackSendAckThread();
                }
            }


            //Fixed Update is configured to run 10 times per second
            void FixedUpdate()
            {
                Network.Service();
            }

            private void OnDestroy()
            {
                Network.Client.RemoveCallbackTarget( this );
                Network.Dispose();
            }

            public void SendPing()
            {
                if( Network == null || Network.Client == null )
                {
                    return;
                }

                Network.Client.OpWebRpc( "ping", null, false );
            }

            public void JoinOrChangeRoom( string roomName )
            {
                if( Network == null )
                {
                    Debug.LogError( "Tried to join room but Network is null" );
                    return;
                }
                if( Network.Client == null )
                {
                    Debug.LogError( "Tried to join room but Network.Client is null" );
                    return;
                }

                

                if( Network.Client.InRoom )
                {
                    Network.Client.OpLeaveRoom( false );
                    m_JoinRoomNameOnLeftRoom = roomName;
                }
                else
                {
                    RaiseJoinRoomEvent( roomName );
                }
            }
            
            public void Reconnect()
            {
                if( Network.Client.IsConnected == true )
                {
                    Debug.Log( "Reconnect!" );
                    m_ReconnectOnDisconnect = true;
                    Network.Disconnect();
                }
                else
                {
                    Network.Connect();
                }
            }

            void RaiseJoinRoomEvent( string roomName )
            {
                EnterRoomParams enterRoomParams = new EnterRoomParams();
                enterRoomParams.RoomName = roomName;
                enterRoomParams.RoomOptions = new RoomOptions();
                enterRoomParams.RoomOptions.MaxPlayers = 200;
                enterRoomParams.RoomOptions.PublishUserId = true;
                enterRoomParams.CreateIfNotExists = true;

                Debug.Log("ConferenceNetwork AppID: " + Network.Client.AppId);
                Debug.Log("ConferenceNetwork AppVersion: " + Network.Client.AppVersion);  
                Debug.Log("ConferenceNetwork CloudRegion: " + Network.Client.CloudRegion); 
                Debug.Log("ConferenceNetwork NameServerAddress: " + Network.Client.NameServerAddress); 	 
                Debug.Log("ConferenceNetwork CurrentServerAddress: " + Network.Client.CurrentServerAddress); 
                Debug.Log("ConferenceNetwork MasterServerAddress: " + Network.Client.MasterServerAddress); 
                //Debug.Log("ConferenceNetwork GameServerAddress: " + Network.Client.GameServerAddress);  
                Debug.Log("ConferenceNetwork Server: " + Network.Client.Server);   
                Debug.Log("ConferenceNetwork NickName: " + Network.Client.NickName); 
                Debug.Log("ConferenceNetwork UserId: " + Network.Client.UserId); 
                Debug.Log("ConferenceNetwork CurrentCluster: " + Network.Client.CurrentCluster);  
               
                

                Debug.Log( "Joining Room " + roomName + " Network State: " + Network.Client.State ); 
                Network.Client.OpJoinOrCreateRoom( enterRoomParams );
            }

            public void OnMasterClientSwitched( Player newMasterClient )
            {
                MasterClientSwitchedCallback?.Invoke( newMasterClient );
            }

            public void OnPlayerEnteredRoom( Player newPlayer )
            {
                

                PlayerEnteredRoomCallback?.Invoke( newPlayer );
            }

            public void OnPlayerLeftRoom( Player otherPlayer )
            {
                PlayerLeftRoomCallback?.Invoke( otherPlayer );
            }

            public void OnPlayerPropertiesUpdate( Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps )
            {
                PlayerPropertiesUpdateCallback?.Invoke( targetPlayer, changedProps );
            }

            public void OnRoomPropertiesUpdate( ExitGames.Client.Photon.Hashtable propertiesThatChanged )
            {
                RoomPropertiesUpdateCallback?.Invoke( propertiesThatChanged );
            }

            public void OnEvent( EventData photonEvent )
            {
                EventCallback?.Invoke( photonEvent );
            }

            public void OnFriendListUpdate( List<FriendInfo> friendList )
            {
                FriendListUpdateCallback?.Invoke( friendList );
            }

            public void OnCreatedRoom()
            {
                CreatedRoomCallback?.Invoke();
            }

            public void OnJoinedRoom()
            {
                //m_JoinRoomNameOnLeftRoom = "";
               // Debug.Log("ConferenceNetwork CurrentLobby Name: " + Network.Client.CurrentLobby.ToString());   
                Debug.Log("On Joined Room: " + Network.Client.CurrentRoom.ToString());   

                JoinedRoomCallback?.Invoke();
            }

            public void OnLeftRoom()
            {
                LeftRoomCallback?.Invoke();
            }

            public void OnCreateRoomFailed( short returnCode, string message )
            {
                CreateRoomFailedCallback?.Invoke( returnCode, message );
            }            

            public void OnJoinRoomFailed( short returnCode, string message )
            {
                JoinRoomFailedCallback?.Invoke( returnCode, message );
            }

            public void OnJoinRandomFailed( short returnCode, string message )
            {
                JoinRandomFailedCallback?.Invoke( returnCode, message );
            }

            public void OnConnected()
            {
                Debug.Log( "OnConnected" );
                ConnectedCallback?.Invoke();
            }

            public void OnConnectedToMaster()
            {
                Debug.Log( "OnConnectedToMaster " + m_JoinRoomNameOnLeftRoom);
                ConnectedToMasterCallback?.Invoke();

                if( string.IsNullOrEmpty( m_JoinRoomNameOnLeftRoom ) == false )
                {
                    RaiseJoinRoomEvent( m_JoinRoomNameOnLeftRoom );
                }
            }

            public void OnDisconnected( DisconnectCause cause )
            {
                Debug.Log( "OnDisconnected: " + cause + ". Reconnect? " + m_ReconnectOnDisconnect );
                DisconnectedCallback?.Invoke( cause );

                if( m_ReconnectOnDisconnect == true )
                {
                    m_ReconnectOnDisconnect = false;
                    Network.Connect();
                }
            }

            public void OnRegionListReceived( RegionHandler regionHandler )
            {
                //throw new System.NotImplementedException();
            }

            public void OnCustomAuthenticationResponse( Dictionary<string, object> data )
            {
                Debug.Log( "OnCustomAuthenticationResponse" );

                var propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                if( data.ContainsKey( "CompanyName" ) )
                {
                    PlayerPrefs.SetString( ConferenceCustomProperties.CompanyNamePropertyName, data[ "CompanyName" ].ToString() );
                }

                List<string> publicProperties = new List<string>();
                publicProperties.Add( "CompanyName" );
                publicProperties.Add( "LinkedIn" );
                publicProperties.Add( "Facebook" );
                publicProperties.Add( "Twitter" );
                publicProperties.Add( "PublicEmail" );
                publicProperties.Add( "FirstName" );
                publicProperties.Add( "LastName" );

                foreach( var pair in data )
                {
                    Debug.Log( pair.Key + ": " + pair.Value );
                    if( publicProperties.Contains( pair.Key ) )
                    {
                        propertiesToSet.Add( pair.Key, pair.Value );
                    }
                }

                Network.Client.LocalPlayer.SetCustomProperties( propertiesToSet );

                if( data.ContainsKey( "IsAdmin" ) )
                {
                    IsAdmin = (bool)data[ "IsAdmin" ];
                }

#if UNITY_EDITOR
                //IsAdmin = true;
#endif
            }

            public void OnCustomAuthenticationFailed( string debugMessage )
            {
                PlayerPrefs.SetString( "OnCustomAuthenticationFailedMessage", debugMessage );
                SceneManager.LoadScene( 0 );
                //throw new System.NotImplementedException();
            }

            public void OnWebRpcResponse( OperationResponse response )
            {
                WebRpcCallback?.Invoke( response );
            }
        }
    }
}