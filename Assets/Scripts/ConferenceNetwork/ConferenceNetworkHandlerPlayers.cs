using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceNetworkHandlerPlayers : ConferenceNetworkHandlerBase, IInRoomCallbacks
        {
            public ConferenceNetworkHandlerPlayers( ConferenceNetwork network ) : base( network )
            {

            }

            public void OnMasterClientSwitched( Player newMasterClient )
            {
                //throw new System.NotImplementedException();
            }

            public void OnPlayerEnteredRoom( Player newPlayer )
            {
                Debug.Log( "OnPlayerEnteredRoom: " + newPlayer.NickName ); 
            }

            public void OnPlayerLeftRoom( Player otherPlayer )
            {
                Debug.Log( "OnPlayerLeftRoom: " + otherPlayer.NickName );
            }

            public void OnPlayerPropertiesUpdate( Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps )
            {
               // throw new System.NotImplementedException();
            }

            public void OnRoomPropertiesUpdate( ExitGames.Client.Photon.Hashtable propertiesThatChanged )
            {
                //throw new System.NotImplementedException();
            }
        }
    }
}