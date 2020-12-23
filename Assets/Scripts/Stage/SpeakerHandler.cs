using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SpeakerHandler : MonoBehaviour
    {
        StageHandlerBase m_ConnectionHandler;

        public static bool IsSpeaker = false;

        float m_NextXRUpdate;

        public float XRUpdateDelay = 0.1f;

        public AvatarXR AvatarXR;
        public AvatarCustomization AvatarCustomization;

        public Transform AvatarStagePosition;
        public Transform AvatarVisitorPosition;

        public GameObject StageInterface;
        public GameObject VisitorInterface;

        void Awake()
        {
            m_ConnectionHandler = GetComponent<StageHandlerBase>();
            if( m_ConnectionHandler == null )
            {
                Debug.LogError( "Could not find StageChatConnection component on gameobject", gameObject );
                DestroyImmediate( this );
                return;
            }

            if( IsSpeaker == false )
            {
                m_ConnectionHandler.SpeakerXRCallback += OnGetXRUpdates;
            }

            m_ConnectionHandler.SpeakerCustomizationCallback += OnGetCustomization;
            SetSpeaker( IsSpeaker );
        }


        private void OnDestroy()
        {
            m_ConnectionHandler.SpeakerCustomizationCallback -= OnGetCustomization;
        }
        [ContextMenu( "SetSpeakerTrue" )]
        void SetSpeakerTrue()
        {
            SetSpeaker( true );

        }
        [ContextMenu( "SetSpeakerFalse" )]
        void SetSpeakerFalse()
        {
            SetSpeaker( false );
        }

        void SetSpeaker( bool value )
        {
            IsSpeaker = value;
            var targetTransform = value ? AvatarStagePosition : AvatarVisitorPosition;

            if(StageInterface != null)
                StageInterface.SetActive( value );

            if(VisitorInterface != null)
                VisitorInterface.SetActive( !value );

            SetSpeakerVisible( !value );

            StopAllCoroutines();

            if( value )
            {
                m_ConnectionHandler.RoomJoinedCallback += SendSpeakerWhenConnected;
            }
        }

        void SetSpeakerVisible( bool value )
        {
            AvatarXR.gameObject.SetActive( value );
        }
        bool IsSpeakerVisible()
        {
            return AvatarXR.gameObject.activeSelf;
        }

        void SendSpeakerWhenConnected()
        {
            Debug.Log( "send speaker customization" );
            m_ConnectionHandler.SendSpeakerCustomization( CustomizationData.Instance.GetCustomizationAsHashtable() );
            m_ConnectionHandler.RoomJoinedCallback -= SendSpeakerWhenConnected;
        }

        void UpdateSpeaker()
        {
            m_ConnectionHandler.UpdateSpeakerPing();

            if ( m_ConnectionHandler.IsInRoom() == false )
            {
                return;
            }

            if( Time.realtimeSinceStartup < m_NextXRUpdate )
            {
                return;
            }

            m_NextXRUpdate = Time.realtimeSinceStartup + XRUpdateDelay;
            XRSystem xr = ViewModeManager.Instance.CurrentViewMode.XRSystem;
            if( xr != null )
            {
                m_ConnectionHandler.SendSpeakerXRUpdate( xr.Serialize() );
            }
        }
       

        void UpdateAttendee()
        {
            if ( m_ConnectionHandler.IsSpeakerPresent() != IsSpeakerVisible() )
            {
                SetSpeakerVisible( m_ConnectionHandler.IsSpeakerPresent() );
            }
        }

        private void Update()
        {
            if( IsSpeaker )
            {
                UpdateSpeaker();
            }
            else
            {
                UpdateAttendee();
            }
        }


        

        void OnGetXRUpdates( ExitGames.Client.Photon.Hashtable data )
        {
            AvatarXR.SetXRSystem( data );
        }

        void OnGetCustomization( ExitGames.Client.Photon.Hashtable data )
        {
            Debug.Log( "Receive Speaker Customization" );
            //AvatarCustomization.OnPlayerPropertiesChanged( data );
            AvatarCustomization.ApplyCustomization();

            var badges = AvatarCustomization.GetComponentsInChildren<AvatarBadge>();
            foreach( var badge in badges )
            {
                //badge.OnPlayerPropertiesChanged( null );
                badge.ApplyNames();
            }
        }

    }
}
