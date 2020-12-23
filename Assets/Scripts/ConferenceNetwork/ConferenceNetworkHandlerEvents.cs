using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceNetworkHandlerEvents : ConferenceNetworkHandlerBase, IOnEventCallback
        {
            public ConferenceNetworkHandlerEvents( ConferenceNetwork network ) : base( network )
            {

            }

            public void OnEvent( EventData photonEvent )
            {
                //throw new System.NotImplementedException();
            }
        }
    }
}