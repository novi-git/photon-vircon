using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace SpaceHub.Conference
{


    public class CameraThirdPerson : MonoBehaviour
    {
        public Transform AvatarRoot;
        public Transform LookTarget;
        public UnityEngine.AI.NavMeshAgent NavAgent;
        public CinemachineFreeLook FreeLookCamera;

        CinemachineObstructionZoom ObstructionZoom;

        public float ZoomSpeed = 3f;
        float m_CameraDistanceTarget;
        float m_CurrentCameraDistance;

        PCRig m_PcRig;

        public bool m_CameraActiveControl;

        public Vector2 ZoomRangeMid;
        public Vector2 ZoomRangeTop;
        public Vector2 ZoomRangeTopHeight;
        public float LookAheadFactor = 0.25f;
        public Vector2 LookAheadDamp;

        public bool isCamera360 = false;

        private void Awake()
        {
            m_PcRig = GetComponentInParent<PCRig>();
            ObstructionZoom = FreeLookCamera.GetComponent<CinemachineObstructionZoom>();
        }

        void Start()
        {
            FreeLookCamera.m_YAxis.Value = 0.5f;
            m_CameraDistanceTarget = 0.5f;
            m_CurrentCameraDistance = m_CameraDistanceTarget;
            PlayerLocal.Instance.GoToAndLookCallback += OnTeleport;

            ConferenceRoomManager.Instance.SceneChangedCallback += OnRoomChanged;
        }



        void OnRoomChanged( string oldRoom, string newRoom)
        {
            FreeLookCamera.m_YAxis.Value = 0.5f;
        }

        void OnTeleport( Transform target, bool teleport, bool useDirection )
        {
            m_CameraActiveControl = false;
            if( teleport )
            {
                StartCoroutine( ResetCameraDelayedRoutine() );
            }
        }

        IEnumerator ResetCameraDelayedRoutine()
        {
            yield return null;
            ResetCameraPosition();
        }

        [ContextMenu( "ResetCameraPosition" )]
        void ResetCameraPosition()
        {
            RotateYToForward( AvatarRoot.forward );
        }

        void RotateYToForward( Vector3 forward )
        {
            m_CameraActiveControl = false;
            FreeLookCamera.m_XAxis.Value = Vector3.SignedAngle( Vector3.forward, AvatarRoot.forward, Vector3.up );
            FreeLookCamera.m_XAxis.Reset();
        }


        void Update()
        {
            if( ConnectingOverlay.IsOpen )
            {
                return;
            }

           /* if (Input.GetKeyDown(KeyCode.V)) {
                isCamera360 = !isCamera360;
                Debug.Log("Camera is " + isCamera360);
            } */

            UpdateLookTarget();
            UpdateRotation();
            UpdateCameraDistance();
        }

        public void OnCamera360(){
            isCamera360 = !isCamera360;
            if(!isCamera360){
                m_PcRig.BtnCam360.sprite = m_PcRig.CamerasIcon[2];
            }else{
                m_PcRig.BtnCam360.sprite = m_PcRig.CamerasIcon[3];
            }
            Debug.Log("Camera is " + isCamera360);
        }
        void UpdateLookTarget()
        {
            var velocity = NavAgent.velocity;
            velocity.y = 0f;

            float zoomFactor = ( GetLookDirection().magnitude / ObstructionZoom.DesiredDistance ) * m_CurrentCameraDistance;
            var targetPoint = NavAgent.transform.position + velocity * LookAheadFactor * zoomFactor + Vector3.up * 1.5f;

            var forward = GetLookDirection();
            forward.y = 0f;
            Plane plane = new Plane( forward, NavAgent.transform.position );
            targetPoint = plane.ClosestPointOnPlane( targetPoint );


            var result = LookTarget.position;
            result.x = Helpers.Damp( result.x, targetPoint.x, Mathf.Lerp( LookAheadDamp.x, LookAheadDamp.y, zoomFactor ) );
            result.z = Helpers.Damp( result.z, targetPoint.z, Mathf.Lerp( LookAheadDamp.x, LookAheadDamp.y, zoomFactor ) );
            result.y = Helpers.Damp( result.y, targetPoint.y, 5f);

            LookTarget.position = result;
        }

        void UpdateCameraDistance()
        {
            m_CameraDistanceTarget -= Input.GetAxis( "Mouse ScrollWheel" ) * ZoomSpeed;
            m_CameraDistanceTarget = Mathf.Clamp01( m_CameraDistanceTarget );

            m_CurrentCameraDistance = Helpers.Damp( m_CurrentCameraDistance, m_CameraDistanceTarget, 3f );

            FreeLookCamera.m_Orbits[ 0 ].m_Height = Mathf.Lerp( ZoomRangeTopHeight.x, ZoomRangeTopHeight.y, m_CurrentCameraDistance );
            FreeLookCamera.m_Orbits[ 0 ].m_Radius = Mathf.Lerp( ZoomRangeTop.x, ZoomRangeTop.y, m_CurrentCameraDistance );
            FreeLookCamera.m_Orbits[ 1 ].m_Radius = Mathf.Lerp( ZoomRangeMid.x, ZoomRangeMid.y, m_CurrentCameraDistance );
        }

        void UpdateRotation()
        {
            FreeLookCamera.m_RecenterToTargetHeading.m_enabled = !m_CameraActiveControl;

            

#if UNITY_ANDROID || UNITY_IOS
            var controls = Input.GetKey( KeyCode.Mouse0 );
#else
            var controls = Input.GetKey( KeyCode.Mouse1 );
#endif

            if( controls && ConferenceSceneSettings.Instance.IsLookEnabled() )
            {
                FreeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                FreeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";

                if( FreeLookCamera.m_XAxis.m_InputAxisValue > 0.2f && FreeLookCamera.m_YAxis.m_InputAxisValue > 0.2f)
                    m_CameraActiveControl = true;
            }
            else
            {
                FreeLookCamera.m_XAxis.m_InputAxisName = "";
                FreeLookCamera.m_YAxis.m_InputAxisName = "";

                FreeLookCamera.m_XAxis.m_InputAxisValue = 0f;
                FreeLookCamera.m_YAxis.m_InputAxisValue = 0f;
                m_CameraActiveControl = false;
            }

            if( m_PcRig.IsMoving() == false && m_CameraActiveControl && !isCamera360)
            {
                Vector3 dir = GetLookDirection();
                dir.y = 0f;
                dir.Normalize();
                m_PcRig.SetLookDirection( dir );
            }

            
        }

        Vector3 GetLookDirection()
        {
            return AvatarRoot.position - FreeLookCamera.VirtualCameraGameObject.transform.position;
        }
    }
}