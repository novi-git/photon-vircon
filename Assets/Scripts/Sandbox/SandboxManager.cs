using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SandboxManager : MonoBehaviour, IOnEventCallback, IConferenceCallbacks
    {
        public static SandboxManager Instance;

        Dictionary<byte, Sandbox> m_ActiveSandboxes = new Dictionary<byte, Sandbox>();

        RaiseEventOptions m_SandboxEventOptions = new RaiseEventOptions();
        Dictionary<byte, object> m_SandboxEventData = new Dictionary<byte, object>();

        private void Awake()
        {
            Instance = this;
            m_SandboxEventData.Add( (byte)0, (byte)0 );
            m_SandboxEventData.Add( (byte)1, (byte)0 );
            m_SandboxEventData.Add( (byte)2, (byte)0 );
            m_SandboxEventData.Add( (byte)3, null );
        }

        IEnumerator Start()
        {
            if( PlayerLocal.Instance.Connector == null )
            {
                yield break;
            }

            PlayerLocal.Instance.Client.AddCallbackTarget( this );
            PlayerLocal.Instance.Connector.Network.AddCallbackTarget( this );
        }

        void Update()
        {
            foreach( var sandbox in m_ActiveSandboxes )
            {
                sandbox.Value.OnUpdate();
            }
        }

        public void ActivateSandbox( Sandbox sandbox )
        {
            m_ActiveSandboxes.Add( sandbox.InterestGroup, sandbox );

            RaiseSandboxEvent( ConferenceEvent.SandboxJoin, sandbox.InterestGroup, sandbox.InterestGroup );
        }

        public void DeactivateSandbox( Sandbox sandbox )
        {
            m_ActiveSandboxes.Remove( sandbox.InterestGroup );

            StartCoroutine( CallPostDeactivateUpdateOnSandbox( sandbox ) );
        }

        IEnumerator CallPostDeactivateUpdateOnSandbox( Sandbox sandbox )
        {
            float time = Time.realtimeSinceStartup;

            while( Time.realtimeSinceStartup - time < 2f )
            {
                sandbox.OnUpdate();
                yield return null;
            }
        }

        public void OnEvent( EventData photonEvent )
        {
            switch( photonEvent.Code )
            {
            case ConferenceEvent.SandboxJoin:
                OnSandboxJoinEvent( photonEvent );
                break;
            case ConferenceEvent.SandboxRpc:
                OnSandboxRpcEvent( photonEvent );
                break;
            }
        }
        void OnSandboxResetEvent( EventData photonEvent )
        {
            var interestGroup = (byte)photonEvent.CustomData;
            Debug.Log( "OnSandboxResetEvent " + interestGroup );
            if( m_ActiveSandboxes.ContainsKey( interestGroup ) )
            {
                m_ActiveSandboxes[ interestGroup ].ResetAllObjects();
            }
        }

        void OnSandboxJoinEvent( EventData photonEvent )
        {
            var interestGroup = (byte)photonEvent.CustomData;

            if( m_ActiveSandboxes.ContainsKey( interestGroup ) )
            {
                m_ActiveSandboxes[ interestGroup ].OnPlayerJoin();
            }
        }

        void OnSandboxRpcEvent( EventData photonEvent )
        {
            var data = (Dictionary<byte, object>)photonEvent.CustomData;

            byte interestGroup = (byte)data[ (byte)0 ];
            byte objectIndex = (byte)data[ (byte)1 ];
            byte rpcCode = (byte)data[ (byte)2 ];
            object[] rpcParameters = (object[])data[ (byte)3 ];

            if( m_ActiveSandboxes.ContainsKey( interestGroup ) )
            {
                m_ActiveSandboxes[ interestGroup ].Objects[ objectIndex ].OnRpc( rpcCode, rpcParameters );
            }
        }

        public void OnSerialize( LoadBalancingClient client )
        {
            foreach( var sandbox in m_ActiveSandboxes )
            {
               
                m_SandboxEventData[ (byte)0 ] = sandbox.Key;

                for( byte j = 0; j < sandbox.Value.Objects.Length; ++j )
                {
                    m_SandboxEventData[ (byte)1 ] = j;

                    foreach( var rpc in sandbox.Value.Objects[ j ].RpcQueue )
                    {
                        m_SandboxEventData[ (byte)2 ] = rpc.RpcCode;
                        m_SandboxEventData[ (byte)3 ] = rpc.Parameters;

                        RaiseSandboxEvent( ConferenceEvent.SandboxRpc, m_SandboxEventData, sandbox.Key );
                    }

                    sandbox.Value.Objects[ j ].RpcQueue.Clear();
                }                
            }            
        }

        void RaiseSandboxEvent( byte eventCode, object customEventContent, byte interestGroup, ReceiverGroup receivers = ReceiverGroup.Others )
        {
            m_SandboxEventOptions.InterestGroup = interestGroup;
            m_SandboxEventOptions.Receivers = receivers;

            PlayerLocal.Instance.Client.OpRaiseEvent( eventCode, customEventContent, m_SandboxEventOptions, SendOptions.SendReliable );
        }

        public void OnSerializeHighFrequency( LoadBalancingClient client )
        {
            
        }

        public void OnSerializeLowFrequency( LoadBalancingClient client )
        {
            
        }
    }
}