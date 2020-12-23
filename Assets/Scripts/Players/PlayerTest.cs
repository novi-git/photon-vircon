using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.AI;

namespace SpaceHub.Conference
{
    public class PlayerTest : PlayerBase, IOnEventCallback
    {
        public bool GotoRandomPosition = true;
        public float GotoRandomPositionTime = 1.5f;

        public bool TeleportOnlyOnce = false;
        public bool CopyLocalPlayerCharacterSystem = false;

        float m_LastTeleport = 0f;

        new void Start()
        {
            base.Start();

            Client.AddCallbackTarget( this );
            
            Player.NickName = "6" + Random.Range( 10000, 99999 );

            var properties = new ExitGames.Client.Photon.Hashtable();

            properties.Add( ConferenceCustomProperties.NickNamePropertyName, Player.NickName );
            properties.Add( ConferenceCustomProperties.CompanyNamePropertyName, "IOI Bot Inc." );
            properties.Add( ConferenceCustomProperties.CallStatusPropertyName, ConferenceCustomProperties.CallStatusType.DoNotDisturb );
            properties.Add( "Facebook", "holocafe" );
            properties.Add( "Twitter", "holocafe" );
            properties.Add( "PublicEmail", "hello@holocafe.de" );
            properties.Add( "LinkedIn", "oliver-eberlei-00b07661" );
            properties.Add( "FirstName", "(I am a" );
            properties.Add( "LastName", "Bot)" );


            Player.SetCustomProperties( properties );
            ConferenceRoomManager.Instance.SceneChangedCallback += OnChangeRoom;

            Connector.ConnectedToMasterCallback += OnConnectedToMaster;
        }

        void OnConnectedToMaster()
        {
            Connector.JoinOrChangeRoom( ConferenceRoomManager.Instance.CurrentRoomName );
        }

        void OnChangeRoom( string oldRoom, string newRoom )
        {
            Destroy( this );
        }

        new void OnDestroy()
        {
            if( Connector.Network != null && Connector.Network.Client != null )
            {
                Connector.Network.Client.RemoveCallbackTarget( this );
            }

            base.OnDestroy();
        }

        public void OnEvent( EventData photonEvent )
        {
            switch( photonEvent.Code )
            {
            case ConferenceEvent.UpdateXRSystem:
                if( CopyLocalPlayerCharacterSystem == true )
                {
                    AvatarManager.Instance.HandleUpdateXrSystemEvent( photonEvent );
                }
                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if( Connector.Network.Client.InRoom == false )
            {
                return;
            }

            if( GotoRandomPosition == true )
            {
                UpdateRandomTeleport();
            }

            if( CopyLocalPlayerCharacterSystem == true )
            {

            }
        }

        void SendCallRequestToLocalPlayer()
        {
            Debug.Log( "SendCallRequestToLocalPlayer" );

            PrivateCalls.SendCallRequestTo( PlayerLocal.Instance.Player );
        }

        void UpdateRandomTeleport()
        {
            if( Time.realtimeSinceStartup - m_LastTeleport < GotoRandomPositionTime )
            {
                return;
            }

            if( TeleportOnlyOnce == true && m_LastTeleport != 0 )
            {
                return;
            }

            TeleportToRandomPosition();
        }

        void TeleportToRandomPosition()
        {
            m_LastTeleport = Time.realtimeSinceStartup;

            float radius = 4f;
            //float boothRadius = 20f;
            Vector3 newPosition = transform.position + new Vector3( Random.Range( -radius, radius ), 0f, Random.Range( -radius, radius ) );
            newPosition = Helpers.ClampToBoothSize( newPosition );
            newPosition.y = 0;


            NavMeshHit hit;

            if( NavMesh.SamplePosition( newPosition, out hit, radius * 2, 1 << NavMesh.GetAreaFromName( "Walkable" ) ) )
            {
                SendRotation( Quaternion.LookRotation( newPosition - transform.position ) );
                SendPosition( hit.position );
            }
        }
    }
}