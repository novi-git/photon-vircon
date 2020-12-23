using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceNetworkHandlerConnections : ConferenceNetworkHandlerBase, IConnectionCallbacks
        {
            string JoinRoomNameOnLeftRoom;

            public ConferenceNetworkHandlerConnections( ConferenceNetwork network ) : base( network )
            {
                Debug.Log( "ExpoNetworkHandler Constructor" );
            }

            public void OnConnected()
            {
                Debug.Log( "OnConnected: " + m_Network.Client.LocalPlayer.NickName );
            }

            public void OnConnectedToMaster()
            {
                string roomName = ConferenceRoomManager.Instance.GetCurrentRoomName();
                Debug.Log( "OnConnectedToMaster " + roomName);

               //  ConferenceRoomManager.Instance.OnChangedScene += JoinRoom;

               // string roomName = ExpoRoomManager.Instance.GetCurrentRoomName();
               /*  if( string.IsNullOrEmpty( roomName ) == false )
                {
                    m_Network.RoomHandler.JoinOrChangeRoom( roomName );
                } */

            }

            public void OnCustomAuthenticationFailed( string debugMessage )
            {
                //Debug.Log( "OnCustomAuthenticationFailed: " + debugMessage );
            }

            public void OnCustomAuthenticationResponse( Dictionary<string, object> data )
            {
               // Debug.Log( "OnCustomAuthenticationResponse" );
            }

            public void OnDisconnected( DisconnectCause cause )
            {
                DebugPanel.DisconnectCauseRealtime = cause.ToString();
                Debug.Log( "OnDisconnected: " + cause );
            }

            public void OnRegionListReceived( RegionHandler regionHandler )
            {
                Debug.Log( "OnRegionListReceived" );
            }
        }
    }
}