using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public interface IConferenceCallbacks
    {
        void OnSerializeLowFrequency( LoadBalancingClient client );

        void OnSerializeHighFrequency( LoadBalancingClient client );

        void OnSerialize( LoadBalancingClient client );
    }
}