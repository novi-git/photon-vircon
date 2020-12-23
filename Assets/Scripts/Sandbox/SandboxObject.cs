using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SandboxObject : MonoBehaviour
    {
        protected delegate void RpcMethod( object[] parameters );

        public class SandboxRpc
        {
            public byte RpcCode;
            public object[] Parameters;
        }

        protected List<RpcMethod> m_RpcMethods = new List<RpcMethod>();
        protected Sandbox m_Sandbox;
        public Queue<SandboxRpc> RpcQueue { get; private set; } = new Queue<SandboxRpc>();
        
        protected void Awake()
        {
            m_Sandbox = GetComponentInParent<Sandbox>();
        }

        private void Start()
        {
            OnRegisterInitialState();
        }

        public void Reset()
        {
            OnReset();
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnPlayerJoin()
        {

        }

        protected virtual void OnRegisterInitialState()
        {

        }

        protected virtual void OnReset()
        {

        }

        protected void Rpc( byte rpcCode, params object[] parameters )
        {
            OnRpc( rpcCode, parameters );
            RpcQueue.Enqueue( new SandboxRpc() { RpcCode = rpcCode, Parameters = parameters } );
        }

        protected byte RegisterRpc( RpcMethod method )
        {
            byte rpcMethodIndex = (byte)m_RpcMethods.Count;
            m_RpcMethods.Add( method );

            return rpcMethodIndex;
        }

        public void OnRpc( byte rpcCode, object[] parameters )
        {
            m_RpcMethods[ rpcCode ]( parameters );
        }
    }
}