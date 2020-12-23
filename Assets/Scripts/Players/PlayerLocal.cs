using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace SpaceHub.Conference
{
    public class PlayerLocal : PlayerBase, IInRoomCallbacks
    {
        public static PlayerLocal Instance;

        public ConferenceVoiceConnection VoiceConnection;

        public UnityAction<Transform, bool, bool> GoToAndLookCallback;

        Vector3 m_LastPosition;
        byte m_FullBodyInterestGroup = 0;
        float m_LastPingTime = 0;

        public AvatarAnimator AvatarAnimator { get; private set; }

        public Vector3 CurrentPosition { get { return m_CurrentPosition; } }

        new void OnEnable()
        {
            Instance = this;

            base.OnEnable();

            if( VersionData.Current == null )
            {
                StartCoroutine( VersionData.Load() );
            }

            AvatarAnimator = GetComponentInChildren<AvatarAnimator>();
        }


        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            VoiceConnection.ConnectAndSetupForPlayer( this );
            CustomProperties.CallStatus = ConferenceCustomProperties.CallStatusType.Available;
        }

        public static bool IsPlayerTyping()
        {
            bool isTypingInChat = Instance != null && Instance.ChatClient != null && Instance.ChatClient.GroupsManager != null && Instance.ChatClient.GroupsManager.ChatInput != null && Instance.ChatClient.GroupsManager.ChatInput.ChatIsActive;
            bool isInMenu = MainMenuPopup.Instance != null && MainMenuPopup.Instance.IsOpen();
            bool isTypingInPopup = MessagePopupMessage.IsInputFieldInUse();
            return isTypingInChat || isInMenu || isTypingInPopup;
        }

        void UpdatePing()
        {
            if( Time.realtimeSinceStartup - m_LastPingTime < 60f )
            {
                return;
            }

            if( Client.IsConnectedAndReady )
            {
                Connector.SendPing();
                m_LastPingTime = Time.realtimeSinceStartup;
            }
            else
            {
                //if not connected, retry every 10 seconds
                m_LastPingTime = Time.realtimeSinceStartup - 50f;
            }
        }



        private void FixedUpdate()
        {

            Quaternion headRotation = GetHeadRotation();

            if( Quaternion.Angle( headRotation, m_CurrentRotation ) > 360f / 16f )
            {
                SendRotation( headRotation, false );
            }

            UpdatePing();
        }

        Quaternion GetHeadRotation()
        {
            return Quaternion.Euler( 0, ViewModeManager.Instance.CurrentViewMode.MainCamera.transform.rotation.eulerAngles.y, 0 );
        }

        public void ForceSendCurrentPositionAndRotationWithHighAccuracy( byte interestgroup )
        {
            m_CurrentPosition = transform.position;
            m_CurrentRotation = GetHeadRotation();

            base.DoSendCurrentPositionAndRotationWithHighAccuracy( interestgroup );
        }

        public override void OnSerializeLowFrequency( LoadBalancingClient client )
        {
            if( m_FullBodyInterestGroup > 0 )
            {
                return;
            }

            base.OnSerializeLowFrequency( client );
        }

        public override void OnSerializeHighFrequency( LoadBalancingClient client )
        {
            if( m_FullBodyInterestGroup > 0 )
            {
                return;
            }

            base.OnSerializeHighFrequency( client );
        }

        public void OnTeleport()
        {
            SendPosition( transform.position ); 
        }

        public override void OnSerialize( LoadBalancingClient client )
        {
            if( m_FullBodyInterestGroup == 0 )
            {
                return;
            }

            if( ViewModeManager.Instance.CurrentViewMode.SerializeXRSystem() )
            {
                var options = new RaiseEventOptions();
                options.InterestGroup = m_FullBodyInterestGroup;

                client.OpRaiseEvent(
                    ConferenceEvent.UpdateXRSystem,
                    ViewModeManager.Instance.CurrentViewMode.XRSystem.Serialize(),
                    options,
                    SendOptions.SendReliable
                    );
            }
            else
            {
                base.SendPositionAndRotationWithHighAccuracy( transform.position, GetHeadRotation(), false, false, m_FullBodyInterestGroup );
                //ForceSendCurrentPositionAndRotationWithHighAccuracy( m_FullBodyInterestGroup );
            }
        }


        public void EnableFullBodySerialization( byte targetInterestGroup )
        {
            m_FullBodyInterestGroup = targetInterestGroup;
        }

        public void DisableFullBodySerialization()
        {
            m_FullBodyInterestGroup = 0;
        }

        public void GoToAndLook( Vector3 position, bool teleport, bool useDirection )
        {
              
            transform.position = position;
            GoToAndLook( transform, teleport, useDirection );
        }


        public void GoToAndLook( Transform targetTransform, bool teleport, bool useDirection )
        {
            var playerXRSystem = ViewModeManager.Instance.CurrentViewMode.XRSystem;
            /*
            Vector3 planarForward = playerXRSystem.Head.forward;
            planarForward.y = 0;
            planarForward.Normalize();

            Quaternion rotationDifferenceToRoot = Quaternion.FromToRotation( planarForward, playerXRSystem.Root.forward );
            
            //transform.rotation = rotationDifferenceToRoot * targetTransform.rotation;
            
            Vector3 positionDifferenceToRoot = Vector3.zero;
            positionDifferenceToRoot = playerXRSystem.Root.position - playerXRSystem.Head.position;
            positionDifferenceToRoot.y = 0;
            */
            transform.position = targetTransform.position;// + positionDifferenceToRoot;

            SendPosition( transform.position, false );

            GoToAndLookCallback?.Invoke( targetTransform, teleport, useDirection );
        }
        #region New Code
        public void OnPlayerEnteredRoom(Player newPlayer) { 
              GoToAndLook(transform.position, true, true);  
              Debug.Log("Player: " + GetPlayer().NickName + " Is Sending Position: " + transform.position + " To: " + newPlayer.NickName);
            StartCoroutine(SendDataToNewPlayer()); // Send data to new player send the updated position!
        }

        IEnumerator SendDataToNewPlayer() {
            yield return null;
        }

        public void OnPlayerLeftRoom(Player otherPlayer) {
            
        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
            
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
            
        }

        public void OnMasterClientSwitched(Player newMasterClient) {
            
        }
#endregion
        /*
public void TeleportToPlayer( string sceneName, string chatId )
{
   StartCoroutine( TeleportToPlayerRoutine( sceneName, chatId ) );
}*/


        /*
        IEnumerator TeleportToPlayerRoutine( string sceneName, string chatId )
        {
            if( ConferenceRoomManager.Instance.CurrentRoomName != sceneName )
            {
                ConferenceRoomManager.LoadRoom( sceneName, null, null );
                yield return null;

                while( Client.CurrentRoom == null || Client.CurrentRoom.Name != sceneName || Client.IsConnectedAndReady == false )
                {
                    yield return null;
                }

                SignalManager.Instance.SendPrivateSignal( SignalManager.SignalType.RequestTeleportPosition, chatId, null );
            }
            else
            {
                var avatar = AvatarManager.Instance.GetAvatarForPlayerUserId( Groups.Helper.GetIdFromSenderId( chatId ) );
                if( avatar != null )
                {
                    GoToAndLook( avatar.CurrentPosition, true, false );
                }
            }
        }*/

    }
}