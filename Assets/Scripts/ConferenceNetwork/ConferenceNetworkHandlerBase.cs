using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceNetworkHandlerBase
        {
            protected ConferenceNetwork m_Network;

            public ConferenceNetworkHandlerBase( ConferenceNetwork network )
            {
                m_Network = network;

                m_Network.Client.AddCallbackTarget( this );
            }

            public void Dispose()
            {
                m_Network.Client.RemoveCallbackTarget( this );
            }
        }
    }
}