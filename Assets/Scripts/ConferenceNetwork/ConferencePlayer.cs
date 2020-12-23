using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public abstract class ConferencePlayer : MonoBehaviour
    {
        public Player Player
        {
            get
            {
                return GetPlayer();
            }
        }

        public abstract Player GetPlayer();
    }
}