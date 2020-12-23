using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Voice.Unity;

namespace SpaceHub.Conference
{
    public class AvatarManager : MonoBehaviour, IInRoomCallbacks, IOnEventCallback, IMatchmakingCallbacks
    {
        public static AvatarManager Instance;

        public GameObject AvatarPrefab;

        Dictionary<string, Avatar> m_Avatars = new Dictionary<string, Avatar>();
        List<Player> m_CreateAvatarQueue = new List<Player>();
        List<string> m_RemoveAvatarQueue = new List<string>();

        List<string> m_EnterPortalList = new List<string>();

        Dictionary<Avatar, Transform> m_MoveAvatarQueue = new Dictionary<Avatar, Transform>();

        int m_LocalActorNumber;

        protected void Awake()
        {
            Instance = this;
        }

        IEnumerator Start()
        {
            if( PlayerLocal.Instance.Connector == null )
            {
                yield break;
            }

            PlayerLocal.Instance.Client.AddCallbackTarget( this );
            PlayerLocal.Instance.Player.NickName = CustomizationData.Instance.Nickname;
            PlayerLocal.Instance.Player.SetCustomProperties( CustomizationData.Instance.GetCustomizationAsHashtable() );

            ConferenceRoomManager.Instance.SceneChangedCallback += OnChangeRoom;
        }

        protected void OnDestroy()
        {
            if( PlayerLocal.Instance.Connector != null &&
                PlayerLocal.Instance.Connector.Network != null &&
                PlayerLocal.Instance.Connector.Network.Client != null )
            {
                PlayerLocal.Instance.Connector.Network.Client.RemoveCallbackTarget( this );
            }

            ConferenceRoomManager.Instance.SceneChangedCallback -= OnChangeRoom;
        }

        protected void Update()
        {
            if( m_CreateAvatarQueue.Count > 0 )
            {
                foreach( var player in m_CreateAvatarQueue )
                {
                    CreateAvatar( player );
                }

                m_CreateAvatarQueue.Clear();
            }

            if( m_RemoveAvatarQueue.Count > 0 )
            {
                foreach( var actorNumber in m_RemoveAvatarQueue )
                {
                    if( m_Avatars.ContainsKey( actorNumber ) && m_Avatars[ actorNumber ] != null )
                    {
                        Destroy( m_Avatars[ actorNumber ].gameObject );
                    }
                    m_Avatars.Remove( actorNumber );
                }

                m_RemoveAvatarQueue.Clear();
            }

            if( m_MoveAvatarQueue.Count > 0 )
            {
                foreach( var pair in m_MoveAvatarQueue )
                {
                    pair.Key.MoveTo( pair.Value.position );
                    pair.Key.RotateTo( pair.Value.rotation );
                }

                m_MoveAvatarQueue.Clear();
            }
            
        }

        protected void OnChangeRoom( string oldRoom, string newRoom )
        {
            PlayerLocal.Instance.ChatClient.UnregisterCallback( oldRoom, OnChatMessageReceived );

            if( ConferenceSceneSettings.Instance.EnableRemoteAvatars )
            {
                PlayerLocal.Instance.ChatClient.RegisterCallback( newRoom, OnChatMessageReceived );
            }
        }

        protected void OnChatMessageReceived( string senderId, object data )
        {
            var table = (ExitGames.Client.Photon.Hashtable)data;

            Groups.MessageType type = (Groups.MessageType)table[ "type" ];
            if( type != Groups.MessageType.Chat )
            {
                return;
            }

            var userId = Groups.Helper.GetIdFromSenderId( senderId );
            string message = table[ "text" ] as string;
            if( userId == PlayerLocal.Instance.Client.LocalPlayer.UserId )
            {
                PlayerLocal.Instance.ForceSendCurrentPositionAndRotationWithHighAccuracy( 0 );
                PlayerLocal.Instance.ChatPopup.ShowMessage( message );
            }
            else if( m_Avatars.ContainsKey( userId ) )
            {
                m_Avatars[ userId ].ShowChatMessage( message );
            }
            else
            {
                Debug.LogWarning( "Got ChatMessage from UserId " + userId + ", but no Avatar was found with this Id" );
            }
        }

        public void OnMasterClientSwitched( Player newMasterClient )
        {

        }

        public void OnPlayerEnteredRoom( Player newPlayer )
        {
            if( ConferenceSceneSettings.Instance.EnableRemoteAvatars == false )
            {
                return;
            }

            if ( IsUserBlocked(newPlayer))
            {
                return;
            }

            
            Debug.Log("Avatar Manager player Entered the room " + newPlayer.NickName);
            m_CreateAvatarQueue.Add( newPlayer ); 
             PlayerLocal.Instance.ForceSendCurrentPositionAndRotationWithHighAccuracy( 0 );
              //PlayerLocal.Instance.GoToAndLook( spawnPoint.transform, true, true );
            //PlayerLocal.Instance.OnPlayerEnteredRoom( newPlayer );
            // Get the AvatarMovement of the Player and send an update!
            // I need to access the player movement script
        }

        public Avatar GetAvatarForActorNumber( int actorNumber )
        {
            if( PlayerLocal.Instance.Client.InRoom == false || PlayerLocal.Instance.Client.CurrentRoom.Players.ContainsKey( actorNumber ) == false )
            {
                return null;
            }
            var player = PlayerLocal.Instance.Client.CurrentRoom.Players[ actorNumber ];
            if( player == null )
            {
                return null;
            }

            string actorUserId = player.UserId;
            return GetAvatarForPlayerUserId( actorUserId );
        }

        public Avatar GetAvatarForPlayerUserId( string userId )
        {
            if( m_Avatars.ContainsKey( userId ) == false )
            {
                return null;
            }

            return m_Avatars[ userId ];
        }

        protected void CreateAvatar( Player player )
        {
            if( player == PlayerLocal.Instance.Player )
            {
                return;
            }

            var spawnPoint = SpawnPoint.GetDefaultSpawnPosition().transform;   
            //         
            GameObject newAvatarObject = Instantiate( AvatarPrefab, spawnPoint.position, spawnPoint.rotation );
            newAvatarObject.name = AvatarPrefab.name + " " + player.ActorNumber;

            Avatar newAvatar = newAvatarObject.GetComponent<Avatar>();
            newAvatar.m_Movement.m_MoveTo = spawnPoint.position; // set the spawn point for created avatar

            newAvatar.SetPlayer( player );

            if( m_Avatars.ContainsKey( player.UserId ) )
            {
                if( m_Avatars[ player.UserId ] != null )
                {
                    Destroy( m_Avatars[ player.UserId ].gameObject );
                }

                m_Avatars.Remove( player.UserId );
            }

             m_Avatars.Add( player.UserId, newAvatar );
             newAvatar.CustomProperties.OnPlayerPropertiesChanged( player.CustomProperties ); 
             UpdateChatBubblePositionForAvatar( newAvatar ); 
               Debug.Log("Created avatar Name" + player.NickName); 
        }

        protected void UpdateChatBubblePositionForAvatar( Avatar avatar )
        {

            //  Debug.Log( "UpdateChatBubblePositionForAvatar Position: " + avatar.CurrentPosition );

            if( avatar.Player == null || avatar.GetCurrentChatBubble() == 0 )
            {
                return;
            }

            Chatbubble bubble = Chatbubble.FindByInterestGroup( avatar.GetCurrentChatBubble() );

            if( bubble != null )
            {
                Transform targetTransform = bubble.GetTargetTransformForPlayer( avatar.Player );

                m_MoveAvatarQueue.Add( avatar, targetTransform );
            }
        }

        public void OnPlayerLeftRoom( Player otherPlayer )
        {
            if( m_Avatars.ContainsKey( otherPlayer.UserId ) && m_EnterPortalList.Contains( otherPlayer.UserId ) == false )
            {
                m_RemoveAvatarQueue.Add( otherPlayer.UserId );
            }
        }

        public void OnPlayerPropertiesUpdate( Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps )
        {
            string s = "OnPlayerPropertiesUpdate\n";
            foreach( var pair in changedProps )
            {
                s += pair.Key + ": " + pair.Value + "\n";
            }
        //    Debug.Log( s );

            if( m_Avatars.ContainsKey( targetPlayer.UserId ) )
            {
                m_Avatars[ targetPlayer.UserId ].CustomProperties.OnPlayerPropertiesChanged( changedProps );
                m_Avatars[ targetPlayer.UserId ].OnPlayerPropertiesUpdate( changedProps );

                UpdateChatBubblePositionForAvatar( m_Avatars[ targetPlayer.UserId ] );
            }
        }

        public void OnRoomPropertiesUpdate( ExitGames.Client.Photon.Hashtable propertiesThatChanged )
        {
            //throw new System.NotImplementedException();
        }


        protected bool IsUserBlocked( Photon.Realtime.Player player )
        {
            var permissions = PlayerLocal.Instance.ChatClient.GroupsManager.Permissions;
            return ( permissions.IsUserBlocked( Groups.Helper.GetAuthName( player ) ) );
        }


        public void OnEvent( EventData photonEvent )
        {

            if( PlayerLocal.Instance == null ||
                PlayerLocal.Instance.Client == null ||
                PlayerLocal.Instance.Client.CurrentRoom == null ||
                PlayerLocal.Instance.Client.CurrentRoom.Players.ContainsKey( photonEvent.Sender ) == false )
            {
                return;
            }

            var player = PlayerLocal.Instance.Client.CurrentRoom.Players[ photonEvent.Sender ];
            string senderUserId = player.UserId;

            if ( IsUserBlocked(player) )
            {
                return;
            }

            if( m_Avatars.ContainsKey( senderUserId ) == false || m_Avatars[ senderUserId ] == null )
            {
                return;
            }

            switch( photonEvent.Code )
            {
            case ConferenceEvent.UpdatePosition:
                m_Avatars[ senderUserId ].MoveTo( (byte[])photonEvent.CustomData );
                Debug.Log(" UpdatePosition!");
                return;
            case ConferenceEvent.UpdatePositionAndRotation:
                Debug.Log(" UpdatePositionAndRotation!");
                Quaternion targetData = (Quaternion)photonEvent.CustomData;

                m_Avatars[ senderUserId ].MoveTo( new Vector3( targetData.x, targetData.y, targetData.z ) );
                m_Avatars[ senderUserId ].RotateTo( Quaternion.Euler( 0, targetData.w, 0 ) );
                return;
            case ConferenceEvent.UpdateTransform:
                //Debug.Log("Update Transform!");
                ExitGames.Client.Photon.Hashtable transformData = (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData;

                m_Avatars[ senderUserId ].TeleportTo( (Vector3)transformData[ 0 ] ); // instant teleport
                m_Avatars[ senderUserId ].RotateTo( (Quaternion)transformData[ 1 ] );
                return;
            case ConferenceEvent.UpdateXRSystem:
                m_Avatars[ senderUserId ].SetXRSystem( (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData );
                return;
            case ConferenceEvent.EnterPortal:
               // Debug.Log("EnterPortal!");
                Vector3 targetPosition = (Vector3)photonEvent.CustomData;
                //m_Avatars[ senderUserId ].MoveTo( targetPosition );
                StartCoroutine( EnterPortalRoutine( senderUserId, targetPosition ) );
                return;
            }
        }

        protected IEnumerator EnterPortalRoutine( string senderUserId, Vector3 targetPosition )
        {
            m_EnterPortalList.Add( senderUserId );
            var wait = new WaitForSeconds( 0.5f );
            m_Avatars[ senderUserId ].MoveTo( targetPosition );

            while( ( m_Avatars[ senderUserId ].CurrentPosition - targetPosition ).sqrMagnitude > 1.5f )
            {
                yield return wait;
            }

            m_EnterPortalList.Remove( senderUserId );
            m_RemoveAvatarQueue.Add( senderUserId );
        }

        public void HandleUpdateXrSystemEvent( EventData photonEvent )
        {
            foreach( var avatar in m_Avatars )
            {
                avatar.Value.SetXRSystem( (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData );
            }

            //m_Avatars[ photonEvent.Sender ].SetXRSystem( (ExitGames.Client.Photon.Hashtable)photonEvent.CustomData );
        }

        public void OnFriendListUpdate( List<FriendInfo> friendList )
        {

        }

        public void OnCreatedRoom()
        {

        }

        public void OnCreateRoomFailed( short returnCode, string message )
        {

        }

        public void OnJoinedRoom()
        {
            if( ConferenceSceneSettings.Instance.EnableRemoteAvatars == false )
            {
                return;
            }

            foreach( var pair in PlayerLocal.Instance.Connector.Network.Client.CurrentRoom.Players )
            {
                if ( IsUserBlocked(pair.Value) == false )
                {
                    m_CreateAvatarQueue.Add( pair.Value );
                }
            }
        }

        public void OnJoinRoomFailed( short returnCode, string message )
        {

        }

        public void OnJoinRandomFailed( short returnCode, string message )
        {

        }

        public void OnLeftRoom()
        {
            /*
            m_CreateAvatarQueue.Clear();
            m_Avatars.Clear();
            m_RemoveAvatarQueue.Clear();
            m_MoveAvatarQueue.Clear();
            */
        }
    }
}
