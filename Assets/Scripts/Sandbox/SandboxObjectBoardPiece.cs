using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SandboxObjectBoardPiece : SandboxObject
    {
        public float SynchedMovementSpeed = 1;
        public Transform SocketsParent;
        public float SocketSnapDistance;

        byte RpcSendPosition;

        Animator m_Animator;

        Vector3 m_InitialPosition;

        Plane m_DragPlane;
        Camera m_DragCamera;
        bool m_IsDragging;
        bool m_IsSelected;
        Vector3 m_DragStartPosition;
        Vector3 m_DragStartObjectPosition;
        Vector3 m_TargetPosition;
        float m_LastPositionUpdate;
        Vector3 m_LastPositionSent;
        Vector3 m_LastSnapPosition;

        ConferenceInteractable m_Interactable;

        new void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Interactable = GetComponentInChildren<ConferenceInteractable>();

            base.Awake();

            RpcSendPosition = RegisterRpc( SendPositionRpc );
        }

        protected override void OnRegisterInitialState()
        {
            m_InitialPosition = transform.position;
            m_TargetPosition = m_InitialPosition;
        }

        protected override void OnReset()
        {
            m_TargetPosition = m_InitialPosition;
        }

        public override void OnUpdate()
        {
            UpdateDragging();
            UpdatePosition();
        }

        void UpdatePosition()
        {
            if( m_IsDragging == true )
            {
                return;
            }

            transform.position = Vector3.Lerp( transform.position, m_TargetPosition, Time.deltaTime * SynchedMovementSpeed );
        }

        void UpdateDragging()
        {
            if( m_IsDragging == false )
            {
                return;
            }

            Vector3 newPiecePosition = GetDragPosition() + ( m_DragStartObjectPosition - m_DragStartPosition );

            transform.position = m_Sandbox.ConstrainPointToSandbox( newPiecePosition );

            float timeElapsed = Time.realtimeSinceStartup - m_LastPositionUpdate;
            float distanceTravelled = ( m_LastPositionSent - transform.position ).magnitude;

            if( timeElapsed > 0.2f && distanceTravelled > 0.15f )
            {
                SendCurrentPosition();
            }

            SnapPositionToSockets();
        }

        void SendCurrentPosition()
        {
            m_LastPositionUpdate = Time.realtimeSinceStartup;
            m_LastPositionSent = transform.position;
            Rpc( RpcSendPosition, transform.position, true );
        }

        void SnapPositionToSockets()
        {
            if( SocketsParent != null && SocketsParent.childCount > 0 && SocketSnapDistance > 0 )
            {
                for( int i = 0; i < SocketsParent.childCount; ++i )
                {
                    if( Vector3.Distance( SocketsParent.GetChild( i ).position, transform.position ) < SocketSnapDistance )
                    {
                        transform.position = SocketsParent.GetChild( i ).position;

                        if( Vector3.Distance( transform.position, m_LastSnapPosition ) > SocketSnapDistance * 0.5f )
                        {
                            m_LastSnapPosition = transform.position;
                            SendCurrentPosition();
                        }
                        return;
                    }
                }
            }
        }

        public void OnInteractionStart()
        {
            if( m_IsSelected == true )
            {
                return;
            }

            m_DragStartObjectPosition = transform.position;
            m_DragPlane = new Plane( Vector3.up, m_DragStartObjectPosition );

            m_DragCamera = Camera.main;
            m_DragStartPosition = GetDragPosition();
            
            m_LastPositionUpdate = Time.realtimeSinceStartup;
            m_LastPositionSent = transform.position;

            m_IsDragging = true;

            Rpc( RpcSendPosition, transform.position, true );
        }

        public void OnInteractionEnd()
        {
            m_IsDragging = false;

            Rpc( RpcSendPosition, transform.position, false );
        }

        public override void OnPlayerJoin()
        {
            if( ( transform.position - m_InitialPosition ).sqrMagnitude > 0.2f )
            {
                Rpc( RpcSendPosition, transform.position, m_IsSelected );
            }
        }

        void SendPositionRpc( object[] parameters )
        {
            m_TargetPosition = (Vector3)parameters[ 0 ];
            m_IsSelected = (bool)parameters[ 1 ];
            m_Animator.SetBool( "IsSelected", m_IsSelected );
        }

        Vector3 GetDragPosition()
        {
            Ray ray = new Ray();

            if( ViewModeManager.SelectedViewMode == ViewModeManager.ViewMode.XR )
            {
                ray.origin = m_Interactable.GetLastSelectInteractor().transform.position;
                ray.direction = m_Interactable.GetLastSelectInteractor().transform.forward;
            }
            else
            {
                ray = m_DragCamera.ScreenPointToRay( Input.mousePosition );
            }

            float enter;

            if( m_DragPlane.Raycast( ray, out enter ) )
            {
                return ray.origin + enter * ray.direction;
            }

            return Vector3.zero;
        }
    }
}