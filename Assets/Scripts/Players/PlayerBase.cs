using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace SpaceHub.Conference
{
    [RequireComponent( typeof( ConferenceConnector ) )]
    public class PlayerBase : ConferencePlayer, IConferenceCallbacks
    {
        public float HighFrequencyRange = 15f;

        public ConferenceConnector Connector { get; private set; }
        public UnityAction<string> OnChatMessageCallback;
        public byte CurrentHighFrequencyInterestGroupId { get; private set; }

        bool m_UpdateLowFrequency;
        bool m_UpdateHighFrequency;

        protected Vector3 m_CurrentPosition;
        protected Quaternion m_CurrentRotation;

        public LoadBalancingClient Client
        {
            get
            {
                return Connector.Network.Client;
            }
        }

        Groups.ChatConnector m_ChatConnector;
        public Groups.ChatConnector ChatClient
        {
            get
            {
                if( m_ChatConnector == null )
                {
                    m_ChatConnector = GetComponent<Groups.ChatConnector>();
                }
                return m_ChatConnector;
            }
        }

        public ConferenceCustomProperties CustomProperties { get; private set; }
        public ConferencePrivateCalls PrivateCalls { get; private set; }

        public AvatarChatPopup ChatPopup;

        protected void OnEnable()
        {
            Connector = GetComponent<ConferenceConnector>();
            CustomProperties = GetComponent<ConferenceCustomProperties>();
            PrivateCalls = GetComponent<ConferencePrivateCalls>();
            PrivateCalls.SetClient( Client );

            ChatPopup = GetComponentInChildren<AvatarChatPopup>();
        }

        protected void Start()
        {
            Connector.Network.AddCallbackTarget( this );
            Connector.JoinedRoomCallback += OnJoinedRoom;
            Connector.PlayerPropertiesUpdateCallback += OnPlayerPropertiesChanged;
            CustomProperties.Player = Player;
        }

        protected void OnDestroy()
        {
            if( Connector != null && Connector.Network != null )
            {
                Connector.Network.RemoveCallbackTarget( this );
            }
        }

        public void OnPlayerPropertiesChanged( Player player, ExitGames.Client.Photon.Hashtable changedProperties )
        {
            CustomProperties.OnPlayerPropertiesChanged( changedProperties );
        }

        public void ForceUpdate()
        {
            m_UpdateHighFrequency = true;
            m_UpdateLowFrequency = true;
        }

        public void SendPosition( Vector3 position, bool updateLocalPosition = true )
        {
            
            if( position == m_CurrentPosition )
            {
                return;
            }

            if( updateLocalPosition == true )
            {
                transform.position = position;
            }

            m_CurrentPosition = position;
            m_UpdateLowFrequency = true;
            m_UpdateHighFrequency = true;

          //    Player localPlayer = GetPlayer();
           //  Debug.Log("Player: " + localPlayer.NickName + " Is Sending Position: " + position);
            //OnTeleportCallback?.Invoke();

        }

        public void SendEnterPortal( Vector3 position )
        {
            var options = new RaiseEventOptions();
            options.InterestGroup = 0;

            Client.OpRaiseEvent(
                ConferenceEvent.EnterPortal,
                position,
                options,
                SendOptions.SendReliable );

            Client.Service();
        }



        public void SendPositionAndRotationWithHighAccuracy( Vector3 position, Quaternion rotation, bool updateLocalTransform = true, bool forceUpdate = false, byte interestGroup = 0 )
        {
            if( m_CurrentPosition == position && m_CurrentRotation == rotation && forceUpdate == false )
            {
                return;
            }

            if( updateLocalTransform == true )
            {
                transform.position = position;
                transform.rotation = rotation;
            }

            m_CurrentPosition = position;
            m_CurrentRotation = rotation;

            DoSendCurrentPositionAndRotationWithHighAccuracy( interestGroup );
        }

        protected void DoSendCurrentPositionAndRotationWithHighAccuracy( byte interestGroup = 0 )
        {
            var options = new RaiseEventOptions();
            options.InterestGroup = interestGroup;

            var data = new ExitGames.Client.Photon.Hashtable();

            data.Add( 0, m_CurrentPosition );
            data.Add( 1, m_CurrentRotation );

            Connector.Network.Client.OpRaiseEvent(
                ConferenceEvent.UpdateTransform,
                data,
                options,
                SendOptions.SendReliable
                );
        }

        public void SendRotation( Quaternion rotation, bool updateLocalRotation = true )
        {
            if( rotation == m_CurrentRotation )
            {
                return;
            }

            if( updateLocalRotation == true )
            {
                transform.rotation = rotation;
            }

            m_CurrentRotation = rotation;

            m_UpdateHighFrequency = true;
        }

        protected void SendCharacterSystem( XRSystem system )
        {

        }

        public virtual void OnSerializeLowFrequency( LoadBalancingClient client )
        {
            if( m_UpdateLowFrequency == false )
            {
                return;
            }

            m_UpdateLowFrequency = false;

            var options = new RaiseEventOptions();
            options.InterestGroup = 1;

            client.OpRaiseEvent(
                ConferenceEvent.UpdatePosition,
                Helpers.Vector3ToExpoPosition( m_CurrentPosition ),
                options,
                SendOptions.SendReliable
                );
        }

        public virtual void OnSerializeHighFrequency( LoadBalancingClient client )
        {

            if( m_UpdateHighFrequency == false )
            {
                return;
            }


            m_UpdateHighFrequency = false;

            byte newInterrestGroup = Helpers.GetHighFrequencyGroupId( m_CurrentPosition );
            if( newInterrestGroup != CurrentHighFrequencyInterestGroupId )
            {
                RaiseUpdatePositionAndRotationEvent( CurrentHighFrequencyInterestGroupId );
                UpdateHighFrequencyInterestGroup( newInterrestGroup );
            }

            RaiseUpdatePositionAndRotationEvent( newInterrestGroup );
        }

        bool InterestGroupNeedsUpdate()
        {
            byte newGroup = Helpers.GetHighFrequencyGroupId( m_CurrentPosition );

            return ( newGroup != CurrentHighFrequencyInterestGroupId );
        }

        void RaiseUpdatePositionAndRotationEvent( byte interrestGroup )
        {
            var options = new RaiseEventOptions();
            options.InterestGroup = interrestGroup;// Helpers.GetHighFrequencyGroupId( m_CurrentPosition );
            //options.TargetActors = GetActorsInCloseProximity();

            //Vector3 sendVector = m_CurrentPosition;
            //sendVector.y = m_CurrentRotation.eulerAngles.y;

            Quaternion sendQuat;
            sendQuat.x = m_CurrentPosition.x;
            sendQuat.y = m_CurrentPosition.y;
            sendQuat.z = m_CurrentPosition.z;
            sendQuat.w = m_CurrentRotation.eulerAngles.y;

            Connector.Network.Client.OpRaiseEvent(
                ConferenceEvent.UpdatePositionAndRotation,
                sendQuat,
                options,
                SendOptions.SendReliable
                );
        }


        public void UpdateHighFrequencyInterestGroup( byte newGroup )
        {
            if( newGroup != CurrentHighFrequencyInterestGroupId )
            {
                byte oldGroup = CurrentHighFrequencyInterestGroupId;

                ChangeInterestGroups( new byte[] { oldGroup }, new byte[] { newGroup } );

                CurrentHighFrequencyInterestGroupId = newGroup;
            }
        }

        public void ChangeInterestGroups( byte[] groupsToRemove, byte[] groupsToAdd )
        {
            Client.OpChangeGroups( groupsToRemove, groupsToAdd );

            if( groupsToRemove != null )
            {
                foreach( var group in groupsToRemove )
                {
                    if( Sandbox.List.ContainsKey( group ) )
                    {
                        Sandbox.List[ group ].Leave();
                    }
                }
            }

            if( groupsToAdd != null )
            {
                foreach( var group in groupsToAdd )
                {
                    if( Sandbox.List.ContainsKey( group ) )
                    {
                        Sandbox.List[ group ].Join();
                    }
                }
            }
        }

        int[] GetActorsInCloseProximity()
        {
            List<int> actors = new List<int>();

            for( int i = 0; i < Avatar.List.Count; ++i )
            {
                if( Avatar.List[ i ].Player != null &&
                    Vector3.Distance( m_CurrentPosition, Avatar.List[ i ].CurrentPosition ) < 15f )
                {
                    actors.Add( Avatar.List[ i ].Player.ActorNumber );
                }
            }

            return actors.ToArray();
        }

        public virtual void OnSerialize( LoadBalancingClient client )
        {

        }

        public virtual void OnJoinedRoom()
        {
            StartCoroutine( OnJoinedRoomRoutine() );
            CustomProperties.OnPlayerPropertiesChanged( Player.CustomProperties );
        }

        IEnumerator OnJoinedRoomRoutine()
        {
            //Wait a bit to get the first position updates
            yield return new WaitForSeconds( 0.1f );
            //Debug.Log("On Joined Room PlayerBase ");
            if( ConferenceSceneSettings.Instance.EnableRemoteAvatars )
            {
                SendPositionAndRotationWithHighAccuracy( m_CurrentPosition, m_CurrentRotation, false, true );
            }
        }

        public override Player GetPlayer()
        {
            return Client.LocalPlayer;
        }
        
    }
}