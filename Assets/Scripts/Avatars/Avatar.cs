using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class Avatar : ConferencePlayer
    {
        public static List<Avatar> List = new List<Avatar>();
        Player m_Player;

        public Vector3 CurrentPosition { get; private set; }

        public ConferenceCustomProperties CustomProperties { get; private set; }

        public AvatarMovement m_Movement;
        AvatarXR m_XR;

        AvatarCustomization m_Customization;
        AvatarChatPopup m_ChatPopup;
        AvatarMuteIcon m_MuteIcon;
        public AvatarAnimator Animator { get; private set; }

        bool m_UpdateVisuals = false;
        float m_LastHighFrequencyUpdate;

        private void OnEnable()
        {
            List.Add( this );
        }

        private void OnDisable()
        {
            List.Remove( this );
        }

        private void Awake()
        {
            m_Movement = GetComponent<AvatarMovement>();
            m_XR = GetComponent<AvatarXR>();

            m_Customization = GetComponent<AvatarCustomization>();
            m_ChatPopup = GetComponentInChildren<AvatarChatPopup>();
            m_MuteIcon = GetComponentInChildren<AvatarMuteIcon>();

            Animator = GetComponent<AvatarAnimator>();
            CustomProperties = GetComponentInParent<ConferenceCustomProperties>();
        }

        private void Update()
        {
            CurrentPosition = transform.position;

            if( m_UpdateVisuals == true )
            {
                m_Customization?.ApplyCustomization();

                m_UpdateVisuals = false;
            }
        }

        public void OnPlayerPropertiesUpdate( ExitGames.Client.Photon.Hashtable changedProps )
        {
            //m_Customization?.OnPlayerPropertiesChanged( changedProps );
            //m_UpdateVisuals = true;
        }


        // send to server
        public void MoveTo( byte[] expoPosition )
        {
            //Ignore low frequency updates if we recently received a high frequency one (which has better accuracy)
            if( Time.realtimeSinceStartup - m_LastHighFrequencyUpdate < 10f )
            {
                return;
            }

           // Debug.Log("Move Using Byte: " + expoPosition);
            m_Movement.MoveTo( Helpers.ExpoPositionToVector3( expoPosition ) );
            m_XR.DisableXrSynchronization();
        }

        // send to player
        public void MoveTo( Vector3 position )
        {
            m_LastHighFrequencyUpdate = Time.realtimeSinceStartup;

            m_Movement.MoveTo( position );
            m_XR.DisableXrSynchronization();
        }

        public void TeleportTo(Vector3 position){

             m_Movement.TeleportTo( position );
        }

        public void RotateTo( Quaternion rotation )
        {
            m_Movement.RotateTo( rotation );
        }

        public void SetXRSystem( ExitGames.Client.Photon.Hashtable data )
        {
            m_XR.SetXRSystem( data );
            Animator.OnReceivedXRUpdate();
        }

        public void ShowChatMessage( string message )
        {
            m_ChatPopup.ShowMessage( message );
        }

        public byte GetCurrentChatBubble()
        {
            if( Player.CustomProperties.ContainsKey( Chatbubble.CurrentChatBubbleProperty ) )
            {
                return (byte)Player.CustomProperties[ Chatbubble.CurrentChatBubbleProperty ];
            }

            return 0;
        }

        public void SetPlayer( Player player )
        {
            m_Player = player;
            CustomProperties.Player = player;
        }

        public override Player GetPlayer()
        {
            return m_Player;
        }

        public void ShowMuteIcon()
        {
            m_MuteIcon.Show();
        }
    }
}