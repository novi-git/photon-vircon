using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SandboxObjectResetButton : SandboxObject
    {
        byte RpcReset;

        new void Awake()
        {
            base.Awake();

            RpcReset = RegisterRpc( SendResetRpc );
        }

        public void OnClick()
        {
            MessagePopup.ShowConfirm( "Do you want to reset all pieces?", "Yes", "No", DoReset, delegate () { } );
        }

        void DoReset()
        {
            Rpc( RpcReset );
        }

        void SendResetRpc( object[] parameters )
        {
            m_Sandbox.ResetAllObjects();
        }
    }
}