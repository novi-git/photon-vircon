using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class EmoteMessageHandler : MonoBehaviour
    {

        public ParticleSystem[] EmoteSystems;
        public ParticleSystem[] EmoteSystemsLocal;

        StageHandlerBase m_Connection;
        StageAttendeeManager m_AttendeeManager;
        public float ForwardVelocity = 10f;

        void Start()
        {
            m_Connection = GetComponent<StageHandlerBase>();
            m_AttendeeManager = transform.parent.GetComponentInChildren<StageAttendeeManager>();
            if( m_Connection == null )
            {
                Debug.LogError( "Could not find StageChatConnection component on gameobject", gameObject );
                DestroyImmediate( this );
                return;
            }

            m_Connection.EmoteCallback += OnEmote;
        }

        private void OnDestroy()
        {
            m_Connection.EmoteCallback -= OnEmote;
        }

        public void SendEmote( int value )
        {
             m_Connection.SendEmote( (byte)value );
             EmoteSystemsLocal[ value ].Emit( 1 );
            Debug.Log("Send Emote: " + (byte)value);
        }

        void OnEmote( byte emote )
        {
            var trans = m_AttendeeManager.GetRandomSpawnTransform();
            EmoteSystems[ emote ].Emit( new ParticleSystem.EmitParams() { position = trans.position, velocity = trans.forward * ForwardVelocity }, 1 );
        }
    }
}
