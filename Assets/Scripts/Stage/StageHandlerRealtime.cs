using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using ExitGames.Client.Photon;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class StageHandlerRealtime : StageHandlerBase, IOnEventCallback, IMatchmakingCallbacks
    {
        ConferenceConnector m_Connector
        {
            get
            {
                return PlayerLocal.Instance.Connector;
            }
        }

        
        const string PresentationPropertyKey = "presentationUri";
        const string SpeakerPropertyKey = "speakerPlayer";
        const string TimerPropertyKey = "timerUpdate"; 


        LoadBalancingClient m_Client { get { return m_Connector.Network.Client; } }
        int m_Speaker;

        private IEnumerator Start()
        {
            while( IsConnected() == false )
            {
                yield return null;
            }
            //m_Connector.JoinOrChangeRoom( "RealtimeStage" );
            m_Client.AddCallbackTarget( this );
            m_Connector.RoomPropertiesUpdateCallback += OnRoomPropertiesUpdated;
            m_Connector.JoinedRoomCallback += OnJoinedRoom;
            //Everyone is connected to same voice room for now, changing methods soon
            while( PlayerLocal.Instance.Client.CurrentRoom == null )
            {
                yield return null;
            }
           // PlayerLocal.Instance.VoiceConnection.JoinRoom( "StageRealtime" );

        }

        private void OnDestroy()
        {
            m_Client?.RemoveCallbackTarget( this );
            m_Connector.RoomPropertiesUpdateCallback -= OnRoomPropertiesUpdated;
             m_Connector.JoinedRoomCallback -= OnJoinedRoom;
            PlayerLocal.Instance?.VoiceConnection.LeaveRoom();
        }

        void OnRoomPropertiesUpdated( ExitGames.Client.Photon.Hashtable data )
        {

            Debug.Log( "OnRoomProperties Updated - Stage Handler" );
            //Debug.Log(data);
            if( data.ContainsKey( PresentationPropertyKey ) )
            {
                //var key = (int)data[ PresentationPropertyKey ];
                PresentationCallback?.Invoke( (int)data[ PresentationPropertyKey ] );
               // Debug.Log(PresentationPropertyKey + " / " + key);
            }


            if( data.ContainsKey( SpeakerPropertyKey ) )
            {
                int actorNumber = (int)data[ SpeakerPropertyKey ];
                m_Speaker = actorNumber;
                var speaker = m_Client.CurrentRoom.GetPlayer( actorNumber );
                SpeakerCustomizationCallback?.Invoke( speaker.CustomProperties );  
            }
 
            if( data.ContainsKey( TimerPropertyKey ) )
            { 
                TimerCallback?.Invoke( (int)data[ TimerPropertyKey ] ); 
            }

            if( data.ContainsKey("TeamColor") &&  data.ContainsKey("TeamScore")){

               ScoreCallback?.Invoke( data );

            }
        }


        public void OnEvent( EventData photonEvent )
        {
          //  Debug.Log("Photon event: " + photonEvent.Code);
            switch( photonEvent.Code )
            {
                case ConferenceEvent.UpdateXRSystem:
                SpeakerXRCallback?.Invoke( (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData );
                break;
            /* case ExpoEvent.StageSpeakerCustomization:

                 if( m_Connector.Network.Client.CurrentRoom.Players.ContainsKey( photonEvent.Sender ) )
                 {
                     m_Speaker = m_Connector.Network.Client.CurrentRoom.Players[ photonEvent.Sender ];
                 }
                 else
                 {
                     Debug.LogWarning( "Could not get Speaker player from sender id " + photonEvent.Sender );
                 }

                 SpeakerCustomizationCallback?.Invoke( (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData );
                 break;*/
                case ConferenceEvent.StageEmotes:
                 Debug.Log("Stage Emote!!");
                EmoteCallback?.Invoke( (byte)photonEvent.CustomData );
                break;

                case ConferenceEvent.StageScoreBoard:
                Debug.Log("UpdateScoreBoard!");
                // ScoreCallback?.Invoke( (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData );
                break;
            }
        }
         
        public override bool IsConnected()
        {
            return m_Connector != null && m_Connector.Network.Client.IsConnectedAndReady; 
        }

        public override bool IsInRoom()
        {
            return m_Connector != null && IsConnected() && m_Connector.Network.Client.InRoom;
        }


        public override bool IsSpeakerPresent()
        {
            return m_Client != null && m_Client.CurrentRoom != null && m_Client.CurrentRoom.GetPlayer( m_Speaker ) != null;
            //return m_Speaker != null && m_Speaker.IsInactive == false;
        }

        public override int GetAttendeesCount()
        {
            if( m_Client == null || m_Client.IsConnectedAndReady == false || m_Client.CurrentRoom == null )
            {
                return 0;
            }
            
            int count = m_Client.CurrentRoom.PlayerCount;
            if( IsSpeakerPresent() )
            {
                --count;
            }
            return count;
        }

        public override void SendSpeakerCustomization( ExitGames.Client.Photon.Hashtable data )
        {
            //   PlayerLocal.Instance.GetPlayer().SetCustomProperties( data );
            int actorNumber = PlayerLocal.Instance.GetPlayer().ActorNumber;
            Debug.Log( "SendSpeaker Customization Realtime " + actorNumber );
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add( SpeakerPropertyKey, actorNumber );
            m_Client.CurrentRoom.SetCustomProperties( table );
        }

        public override void SendSpeakerXRUpdate( ExitGames.Client.Photon.Hashtable data )
        {
            m_Client.OpRaiseEvent( ConferenceEvent.UpdateXRSystem, data, RaiseEventOptions.Default, SendOptions.SendReliable );
        }

        public override void SendScoreBoard( ExitGames.Client.Photon.Hashtable data ) {
            // Add the data from ScoreManager
            Debug.Log("Send Stage Handler Realtime!");
            m_Client.CurrentRoom.SetCustomProperties( data );
         // m_Client.OpRaiseEvent( ConferenceEvent.StageScoreBoard, data, RaiseEventOptions.Default, SendOptions.SendReliable ); // send this data 
        }

      
        public override void SendPresentationUri( int uri )
        {
            //m_Client.OpRaiseEvent( ExpoEvent.StagePresentationUri, uri, RaiseEventOptions.Default, SendOptions.SendReliable );
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add( PresentationPropertyKey, uri );
            m_Client.CurrentRoom.SetCustomProperties( table );
        }
        public override void SendEmote( byte emote )
        {
             m_Client.OpRaiseEvent( ConferenceEvent.StageEmotes, emote, RaiseEventOptions.Default, SendOptions.SendReliable );
           

        }

        public override void UpdateTimer( int timer ){

            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add( TimerPropertyKey, timer );
            m_Client.CurrentRoom.SetCustomProperties( table ); 
        }

        public void OnJoinedRoom()
        {

            Debug.Log("On Joined Room StageHandlerRealTime");

            var properties = m_Client.CurrentRoom.CustomProperties;

            if( properties.ContainsKey( PresentationPropertyKey ) )
            {
                PresentationCallback?.Invoke( (int)properties[ PresentationPropertyKey ] );
            }

            if( properties.ContainsKey( TimerPropertyKey ) )
            {
                TimerCallback?.Invoke( (int)properties[ TimerPropertyKey ] );
            }

            if( properties.ContainsKey("TeamColor") &&  properties.ContainsKey("TeamScore")){

               ScoreCallback?.Invoke( properties );

            }

            if( properties.ContainsKey( SpeakerPropertyKey ) )
            {
                int actorNumber = (int)properties[ SpeakerPropertyKey ];
                m_Speaker = actorNumber;
                var speaker = m_Client.CurrentRoom.GetPlayer( actorNumber );
                if( speaker != null )
                {
                    SpeakerCustomizationCallback?.Invoke( speaker.CustomProperties );
                }
            }

            RoomJoinedCallback?.Invoke();
        }


        public void OnFriendListUpdate( List<FriendInfo> friendList ) { }
        public void OnCreatedRoom() { }
        public void OnCreateRoomFailed( short returnCode, string message ) { }
        public void OnJoinRoomFailed( short returnCode, string message ) { }
        public void OnJoinRandomFailed( short returnCode, string message ) { }
        public void OnLeftRoom() { }
    }
}
