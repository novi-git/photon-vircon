using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class PCRig : MonoBehaviour
    {
        public Transform TargetObject;
        public Transform AvatarRoot;
        NavMeshAgent m_NavmeshAgent;

        public Transform PathPreviewObject;
        public GameObject TeleportConfirmReticlePrefab;
        public LayerMask TeleportLayerMask;

        public float MovementSpeed;
        float m_CurrentMovementSpeed;

        ConferenceInteractable m_HoverInteractable;
        RaycastHit m_MouseRayHit;
        bool m_IsMouseRayHitValid;

        bool m_IsMouseOverUIOnSelectEnter;
        Vector3 m_TargetPosition;

        public GameObject[] Cameras;
        public Sprite[] CamerasIcon;

        int m_CameraIndex = 0;

        bool m_IsMoving = false;

        Vector3 m_TargetDirection;
        bool m_UseDirection = false;

        public Image BtnCamTopDown;
        public Image BtnCam360;

        CameraThirdPerson cam3rdPerson;

        void Start()
        {
            SetCameraIndexActive();
            transform.SetParent( null );
            PlayerLocal.Instance.GoToAndLookCallback += OnGoTo;
            
            m_NavmeshAgent = transform.GetComponentInChildren<NavMeshAgent>();
            cam3rdPerson = transform.GetComponentInChildren<CameraThirdPerson>();
        }

        void Update()
        {
            UpdateInteraction();

           //UpdateCameraIndex();

            UpdateNavigation();
            UpdateRotation();
        }

        void UpdateNavigation()
        {
            if( m_NavmeshAgent.enabled && 
                m_NavmeshAgent.isOnNavMesh &&
                AvatarRoot != null &&
                TargetObject != null )
            {
                float sqrDistance = Vector3.SqrMagnitude( AvatarRoot.position - TargetObject.position );
                m_NavmeshAgent.speed = Mathf.Min( MovementSpeed, sqrDistance * 1.5f );

                if( m_NavmeshAgent.pathPending == false && m_NavmeshAgent.remainingDistance < 0.1f && sqrDistance > 0.2f )
                {
                    OnGoTo( TargetObject, true, true );
                }
            }
        }

        void UpdateRotation()
        {
            if( m_IsMoving )
            {
                var direction = m_NavmeshAgent.velocity;
                direction.y = 0f;
                AvatarRoot.rotation = Helpers.Damp( AvatarRoot.rotation, Quaternion.LookRotation( direction ), 7f );
            }
            else if( m_UseDirection )
            {
                AvatarRoot.rotation = Helpers.Damp( AvatarRoot.rotation, Quaternion.LookRotation( m_TargetDirection ), 7f );
            }
        }
        void UpdateCameraIndex()
        {
            if( PlayerLocal.IsPlayerTyping() )
            {
                return;
            }

           if( ConferenceSceneSettings.Instance != null && ConferenceSceneSettings.Instance.EnableCameraToggle && Input.GetKeyDown( KeyCode.C ) )
            {
                m_CameraIndex = ( m_CameraIndex + 1 ) % Cameras.Length;
                SetCameraIndexActive();
            }  
        }

        public void ChangeCameraTopDown(){

             if( ConferenceSceneSettings.Instance != null && ConferenceSceneSettings.Instance.EnableCameraToggle){
                 m_CameraIndex = ( m_CameraIndex + 1 ) % Cameras.Length;
                 if(m_CameraIndex == 0){
                     // Off
                    BtnCamTopDown.sprite = CamerasIcon[0];
                 }else{
                     // On
                     BtnCamTopDown.sprite = CamerasIcon[1];
                 }
                 SetCameraIndexActive();
             }
            
        }

        void SetCameraIndexActive()
        {
            for( int i = 0; i < Cameras.Length; ++i )
            {
                Cameras[ i ].SetActive( i == m_CameraIndex );
            }
        }

        public void SetLookDirection( Vector3 dir )
        {
            dir.y = 0f;
            m_TargetDirection = dir;
            m_UseDirection = true;
        }

        public bool IsMoving()
        {
            return m_NavmeshAgent.velocity.sqrMagnitude > 0.5f;
        }
        /*
        void UpdateMovement()
        {
            if( TargetObject == null || AvatarRoot == null )
            {
                return;
            }

            Vector3 direction = TargetObject.position - AvatarRoot.position;
            float targetMovementSpeed = 0f;

            m_IsMoving = direction.sqrMagnitude > 0.01f;
            if( m_IsMoving )
            {
                direction.y = 0f;
                AvatarRoot.rotation = Helpers.Damp( AvatarRoot.rotation, Quaternion.LookRotation( direction ), 7f );

                targetMovementSpeed = MovementSpeed;
            }

            m_CurrentMovementSpeed = Helpers.Damp( m_CurrentMovementSpeed, targetMovementSpeed, 3f );
            m_CurrentMovementSpeed = Mathf.Max( 0.1f, Mathf.Min( m_CurrentMovementSpeed, direction.sqrMagnitude * 3f ) );

            AvatarRoot.position = Vector3.MoveTowards( AvatarRoot.position, TargetObject.position, Time.deltaTime * m_CurrentMovementSpeed );
            if( PathPreviewObject != null )
            {
                PathPreviewObject.position = Helpers.Damp( PathPreviewObject.position, TargetObject.position, 6f );
            }
        }
        */
        void OnGoTo( Transform target, bool teleport, bool useDirection )
        {
            TargetObject = target;
            m_UseDirection = useDirection;
            Vector3 direction = target.forward;
            direction.y = 0;
            if( useDirection )
            {
                m_TargetDirection = direction;
            }

            if( teleport )
            {
                if ( m_NavmeshAgent.isOnNavMesh )
                {
                    m_NavmeshAgent.isStopped = true;
                }
                m_NavmeshAgent.enabled = false;
                m_NavmeshAgent.Warp( target.position );

                AvatarRoot.position = target.position;

                AvatarRoot.rotation = Quaternion.LookRotation( direction );
                PathPreviewObject.position = target.position;
            }
            else
            {
                m_NavmeshAgent.enabled = true;
                m_NavmeshAgent.isStopped = false;
                m_NavmeshAgent.SetDestination( TargetObject.position );
            }
        }


        void UpdateInteraction()
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );

            m_IsMouseRayHitValid = Physics.Raycast( ray, out m_MouseRayHit, 60f, TeleportLayerMask.value, QueryTriggerInteraction.Ignore );

            if( m_IsMouseRayHitValid )
            {
                ConferenceInteractable interactable = m_MouseRayHit.collider.GetComponent<ConferenceInteractable>();

                if( IsPointerOverUIElement() == true )
                {
                    interactable = null;
                }

                if( ConferenceInteractable.Selected != null )
                {
                    interactable = ConferenceInteractable.Selected;
                }

                if( m_HoverInteractable != interactable )
                {
                    if( m_HoverInteractable != null )
                    {
                        m_HoverInteractable.TriggerHoverExit();
                    }

                    if( interactable != null )
                    {
                        interactable.TriggerHoverEnter();
                    }

                    m_HoverInteractable = interactable;
                }
            }
            else if ( m_HoverInteractable != null )
            {
                m_HoverInteractable.TriggerHoverExit();
                m_HoverInteractable = null;
            }

            if( Input.GetKeyDown( KeyCode.Mouse0 ) && !cam3rdPerson.m_CameraActiveControl )
            {
                m_IsMouseOverUIOnSelectEnter = IsPointerOverUIElement();
                if( m_HoverInteractable != null )
                {
                    m_HoverInteractable.TriggerSelectEnter();
                }
            }

            if( Input.GetKeyDown( KeyCode.Mouse0 ) && !cam3rdPerson.m_CameraActiveControl )
            {
                if( m_IsMouseOverUIOnSelectEnter == false && m_IsMouseRayHitValid == true )
                {
                    if( ConferenceInteractable.Selected != null )
                    {
                        ConferenceInteractable.Selected.TriggerSelectExit();
                        m_HoverInteractable = null;
                    }
                    else if( m_HoverInteractable == null )
                    {
                        if( ConferenceSceneSettings.Instance.IsTeleportEnabled() )
                        {
                            NavMeshHit hit;

                            if( NavMesh.SamplePosition( m_MouseRayHit.point, out hit, 2f, NavMesh.AllAreas ) )
                            {
                                //TargetObject.position = hit.position;
                                PlayerLocal.Instance.GoToAndLook( hit.position, false, false );
                                Instantiate( TeleportConfirmReticlePrefab, hit.position, Quaternion.LookRotation( Vector3.Cross( Vector3.forward, m_MouseRayHit.normal ), m_MouseRayHit.normal ) );
                            }
                        }
                    }
                    else
                    {
                        m_HoverInteractable.TriggerSelectExit();
                    }
                }
            }
        }

        public static bool IsPointerOverUIElement()
        {
            var eventData = new PointerEventData( EventSystem.current );
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll( eventData, results );
            return results.Count > 0;
        }


    }
}