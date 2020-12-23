using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceNetwork
        {
            public LoadBalancingClient Client { get; private set; }

            Thread m_Thread;
            public ConferenceNetworkHandlerConnections ConnectionHandler { get; private set; }
            public ConferenceNetworkHandlerRooms RoomHandler { get; private set; }
            public ConferenceNetworkHandlerEvents EventHandler { get; private set; }
            public ConferenceNetworkHandlerPlayers PlayerHandler { get; private set; }

            bool m_IsMessageQueueRunning = true;
            public bool IsMessageQueueRunning
            {
                get
                {
                    return m_IsMessageQueueRunning;
                }

                set
                {
                    Client.LoadBalancingPeer.IsSendingOnlyAcks = !value;
                    m_IsMessageQueueRunning = value;
                }
            }

            bool m_ShouldExit = false;
            List<IConferenceCallbacks> m_Callbacks = new List<IConferenceCallbacks>();
            int m_ServiceFrameCounter = 0;
            bool m_IsTestPlayer;

            public ConferenceNetwork( bool isTestPlayer = false )
            {
                m_IsTestPlayer = isTestPlayer;

                Client = new LoadBalancingClient();
                ConnectionHandler = new ConferenceNetworkHandlerConnections( this );
                RoomHandler = new ConferenceNetworkHandlerRooms( this );
                //m_EventHandler = new ExpoNetworkHandlerEvents( this );
                PlayerHandler = new ConferenceNetworkHandlerPlayers( this );

                CustomTypes.Register();
            }

            ~ConferenceNetwork()
            {
                Dispose();
            }

            public void AddCallbackTarget( IConferenceCallbacks callbackTarget )
            {
                m_Callbacks.Add( callbackTarget );
            }

            public void RemoveCallbackTarget( IConferenceCallbacks callbackTarget )
            {
                m_Callbacks.Remove( callbackTarget );
            }

            public void Dispose()
            {
                if( Client == null )
                {
                    return;
                }

                lock( Client )
                {
                    m_ShouldExit = true;
                    ConnectionHandler.Dispose();
                    RoomHandler.Dispose();
                    //m_EventHandler.Dispose();
                    PlayerHandler.Dispose();
                    Disconnect();
                    Client.Service();

                    Client = null;
                }
            }

            public void Connect()
            {
                string region = PlayerPrefs.GetString( "PhotonRegion", ConferenceServerSettings.Instance.DefaultPhotonRegion );
                Debug.Log( $"Connect to {region}" ); 

                Client.AppId = ConferenceServerSettings.Instance.GetRealtimeAppId();
                Client.AppVersion = ConferenceServerSettings.Instance.GetAppVersion();
                

                if( string.IsNullOrEmpty( PlayerPrefs.GetString( "EventId" ) ) == false )
                {
                    Client.AppVersion += "#" + PlayerPrefs.GetString( "EventId" );
                }

                string loginMail = PlayerPrefs.GetString( "SpaceHubLoginEmail" );
                string loginToken = PlayerPrefs.GetString( "SpaceHubLoginToken" );

                var auth = new AuthenticationValues();
                auth.AuthType = CustomAuthenticationType.Custom;
                auth.AddAuthParameter( "version", VersionData.AppVersion );
                auth.AddAuthParameter( "eventid", PlayerPrefs.GetString( "EventId" ) );

                if( string.IsNullOrWhiteSpace( loginMail ) == false && string.IsNullOrWhiteSpace( loginToken ) == false && m_IsTestPlayer == false )
                {
                    auth.AddAuthParameter( "email", loginMail );
                    auth.AddAuthParameter( "token", loginToken );

                    Debug.Log( "Send authentication request for " + loginMail );
                }
                else
                {
                    auth.AddAuthParameter( "guest", "guest" );
                }

                Client.AuthValues = auth;

                //Client.LoadBalancingPeer.DebugOut = ExitGames.Client.Photon.DebugLevel.ALL;

                //Used to connected to server
                if( !Client.ConnectToRegionMaster( region ) )
                {
                    Debug.LogError( "Couldn't connect to server" );
                    throw new System.Exception( "Couldn't connect to server" );
                } else{
                    Debug.Log( $"Connected to {region}" ); 
                }
            }

            public void Disconnect()
            {
                if( Client.IsConnected == true )
                {
                    Client.Disconnect();
                }
            }

            public void Service()
            {
                if( m_ShouldExit == true )
                {
                    return;
                }

                if( IsMessageQueueRunning == false )
                {
                    return;
                }

                if( Client.InRoom == true )
                {
                    //Service is called 10 times per second, this means high frequency updates are every 2 seconds and low frequency updates are every 20 seconds
                    //bool serializeHighFrequency = m_ServiceFrameCounter % 5 == 0;
                    //bool serializeLowFrequency = m_ServiceFrameCounter % 10 == 0;

                    int updatesPerSecond = 10;

                    CustomLatency.Latency time = CustomLatency.GetLatency();
                    float HighLatencyCount = updatesPerSecond * time.HighLatency;
                    float LowLatencyCount = updatesPerSecond * time.LowLatency;
                    bool serializeHighFrequency = m_ServiceFrameCounter % HighLatencyCount == 0;
                    bool serializeLowFrequency = m_ServiceFrameCounter % LowLatencyCount == 0;


                    for ( int i = 0; i < m_Callbacks.Count; ++i )
                    {
                        m_Callbacks[ i ].OnSerialize( Client );

                        if( serializeHighFrequency )
                        {
                            m_Callbacks[ i ].OnSerializeHighFrequency( Client );
                        }

                        if( serializeLowFrequency )
                        {
                            m_Callbacks[ i ].OnSerializeLowFrequency( Client );
                        }
                    }
                }

                Client.Service();

                m_ServiceFrameCounter++;

                if( m_ServiceFrameCounter > 200 )
                {
                    m_ServiceFrameCounter = 0;
                }

            }
        }
    }
}