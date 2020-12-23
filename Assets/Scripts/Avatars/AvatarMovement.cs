using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace SpaceHub.Conference
{


    public class AvatarMovement : MonoBehaviour
    {
        static Vector3 m_AdditionalAvoidancePosition;
        static float m_AdditionalAvoidanceRadius = -1;
        public static void SetAdditionalAvoidance( Vector3 position, float radius )
        {
            m_AdditionalAvoidancePosition = position;
            m_AdditionalAvoidanceRadius = radius;
        }
        public static void ResetAdditionalAvoidance()
        {
            m_AdditionalAvoidanceRadius = -1f;
        }


        public float MoveToSpeed;
        public float AvoidanceDistance = 1f;

        NavMeshPath m_Path;
        int m_PathIndex;

        public Vector3 m_MoveTo; // start position
        Quaternion m_RotateTo = Quaternion.identity;
        bool m_DoLookAt;

        void Update()
        {
            UpdatePosition();
        }

        void UpdatePosition()
        {
            if( m_Path != null )
            {
                m_MoveTo = m_Path.corners[ m_PathIndex ];
                
                var waypointDelta = transform.position - m_MoveTo;
                if( waypointDelta.sqrMagnitude < 0.5 )
                {
                    m_PathIndex++;
                    if( m_PathIndex >= m_Path.corners.Length )
                    {
                        m_Path = null;
                    }
                }
            }

            if( m_DoLookAt == true )
            {
                //m_RotateTo = Quaternion.LookRotation( m_MoveTo - transform.position, Vector3.up );
                //Vector3 lookAt = m_MoveTo;
                //lookAt.y = transform.position.y;
                //transform.LookAt( lookAt );
                //CalculateLook( m_MoveTo );
                m_DoLookAt = false;

            }

            //if( m_DoRotateTo == true )
            {
                transform.rotation = Helpers.Damp( transform.rotation, m_RotateTo, 5f );
                //transform.rotation = m_RotateTo;
                //m_DoRotateTo = false;
            }

            transform.position = Vector3.MoveTowards( transform.position, m_MoveTo, Time.deltaTime * MoveToSpeed );

            AvoidLocalPlayer();

            if ( m_AdditionalAvoidanceRadius > 0f )
            {
                AvoidPosition( m_AdditionalAvoidancePosition, m_AdditionalAvoidanceRadius );
            }
        }

        void AvoidLocalPlayer()
        {
            Vector3 position = PlayerLocal.Instance.transform.position;

            if( ViewModeManager.Instance != null &&
                ViewModeManager.Instance.CurrentViewMode != null &&
                ViewModeManager.Instance.CurrentViewMode.XRSystem != null &&
                ViewModeManager.Instance.CurrentViewMode.XRSystem.Head != null 
                )
            {
                position = ViewModeManager.Instance.CurrentViewMode.XRSystem.Head.position;
            }

            AvoidPosition( ViewModeManager.Instance.CurrentViewMode.XRSystem.Head.position, AvoidanceDistance );
        }

        void AvoidPosition( Vector3 pos, float radius )
        {
            Vector3 delta = pos - transform.position;
            delta.y = 0f;

            float magnitude = delta.magnitude;

            if( magnitude < radius )
            {
                transform.position -= delta.normalized * ( radius - magnitude );
            }
        }


        public void MoveTo( Vector3 position )
        {
            NavMeshPath path = new NavMeshPath();
            if( NavMesh.CalculatePath( transform.position, position, 1 << NavMesh.GetAreaFromName( "Walkable" ), path ) )
            {
                m_Path = path;
                m_PathIndex = 0;
            }
            else
            {
                m_Path = null;
                m_MoveTo = position;
                //CalculateLook( m_MoveTo );
            }

            //m_DoLookAt = true;
        }

        public void TeleportTo(Vector3 position){ 
            m_Path = null;
            m_MoveTo = position;
            transform.position = position;
           //  
        }

        void CalculateLook( Vector3 target )
        {
            m_RotateTo = Quaternion.LookRotation( target - transform.position, Vector3.up );
        }

        public void RotateTo( Quaternion rotation )
        {
            m_DoLookAt = false;
            m_RotateTo = rotation;
        }
    }
}