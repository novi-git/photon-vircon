using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceNetworkHandlerRooms : ConferenceNetworkHandlerBase, IMatchmakingCallbacks
        {
            string m_JoinRoomNameOnLeftRoom;

            public ConferenceNetworkHandlerRooms( ConferenceNetwork network ) : base( network )
            {

            }

            public void OnFriendListUpdate( List<FriendInfo> friendList )
            {
                Debug.Log( "OnRegionListReceived" );
            }

            public void OnCreatedRoom()
            {
                Debug.Log( "OnCreatedRoom" );
            }

            public void OnCreateRoomFailed( short returnCode, string message )
            {
                Debug.Log( "OnCreateRoomFailed" );
            }

            public void OnJoinedRoom()
            {
                byte[] newGroups = new byte[ 2 ];
                newGroups[ 0 ] = 1;
                newGroups[ 1 ] = 2;

                m_Network.Client.OpChangeGroups( null, newGroups );
            }

            public void OnJoinRoomFailed( short returnCode, string message )
            {
                Debug.LogError( "OnJoinRoomFailed: " + message );
            }

            public void OnJoinRandomFailed( short returnCode, string message )
            {
                Debug.LogError( "OnJoinRandomFailed: " + message );
            }

            public void OnLeftRoom()
            {
                Debug.Log( "OnLeftRoom" );
            }
        }
    }
}