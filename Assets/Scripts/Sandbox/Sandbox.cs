using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    /// <summary>
    /// Refers to the Solitaire like game
    /// </summary>
    public class Sandbox : MonoBehaviour
    {
        public byte InterestGroup;
        public static Dictionary<byte, Sandbox> List = new Dictionary<byte, Sandbox>();

        public SandboxObject[] Objects;
        Collider m_Collider;

        private void Awake()
        {
            m_Collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            if( List.ContainsKey( InterestGroup ) == true )
            {
                Debug.LogError( $"Sandbox {InterestGroup} tries to register, but the same InterestGroup is already in the list.", this );
                Debug.LogError( $" Already registered sandbox is: " + List[ InterestGroup ], List[ InterestGroup ] );
                return;
            }

            List.Add( InterestGroup, this );
        }

        private void OnDisable()
        {
            List.Remove( InterestGroup );
        }

        public void OnUpdate()
        {
            foreach( var sandboxObject in Objects )
            {
                sandboxObject.OnUpdate();
            }
        }

        public void Join()
        {
            SandboxManager.Instance.ActivateSandbox( this );
        }

        public void Leave()
        {
            SandboxManager.Instance.DeactivateSandbox( this );
            ResetAllObjects();
        }

        public void ResetAllObjects()
        {
            foreach( var sandboxObject in Objects )
            {
                sandboxObject.Reset();
            }
        }

        public void OnPlayerJoin()
        {
            foreach( var sandboxObject in Objects )
            {
                sandboxObject.OnPlayerJoin();
            }
        }

        public Vector3 ConstrainPointToSandbox( Vector3 position )
        {
            if( m_Collider == null )
            {
                return position;
            }

            return m_Collider.ClosestPoint( position );
        }

        public void Deserialize()
        {

        }

        void OnDrawGizmos()
        {
            // Draws the Light bulb icon at position of the object.
            // Because we draw it inside OnDrawGizmos the icon is also pickable
            // in the scene view.

        }
    }
}