using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public abstract class StageHandlerBase : MonoBehaviour
    {
        public UnityAction<ExitGames.Client.Photon.Hashtable> SpeakerXRCallback;
        public UnityAction<ExitGames.Client.Photon.Hashtable> SpeakerCustomizationCallback; 
        public UnityAction<ExitGames.Client.Photon.Hashtable> ScoreCallback;

        public UnityAction<byte> EmoteCallback;
        public UnityAction<int> PresentationCallback;
        public UnityAction<int> TimerCallback;


        public UnityAction RoomJoinedCallback;

        public abstract bool IsConnected();
        public abstract bool IsInRoom();

        public abstract bool IsSpeakerPresent();

        public abstract void SendSpeakerCustomization( ExitGames.Client.Photon.Hashtable data );
        public abstract void SendSpeakerXRUpdate( ExitGames.Client.Photon.Hashtable data );

        public abstract void SendEmote( byte emote );
        public abstract void SendPresentationUri( int uri );
        public abstract void UpdateTimer( int timer );
        public abstract void SendScoreBoard( ExitGames.Client.Photon.Hashtable data ); // Team and Integer

        public abstract int GetAttendeesCount();

        public virtual void UpdateSpeakerPing() { }
    }
}

