#if !UNITY_2019_3_OR_NEWER
#define CINEMACHINE_PHYSICS
#define CINEMACHINE_PHYSICS_2D
#endif

using UnityEngine;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine.Serialization;
using System;
using UnityEngine.SceneManagement;

namespace Cinemachine
{
    [DocumentationSorting( DocumentationSortingAttribute.Level.UserRef )]
    [AddComponentMenu( "" )] // Hide in menu
    [SaveDuringPlay]
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    public class CinemachineObstructionZoom : CinemachineExtension
    {
        public LayerMask m_CollideAgainst = 1;
        public Transform Avatar;
        public float m_MinimumDistance = 0f;
        public float m_SmoothingIn = 0f;
        public float m_SmoothingOut = 0f;


        public float m_CameraRadius = 0.1f;


        private void OnValidate()
        {
            m_MinimumDistance = Mathf.Max( 0, m_MinimumDistance );
            m_SmoothingIn = Mathf.Max( 0.01f, m_SmoothingIn );
            m_SmoothingOut = Mathf.Max( 0.01f, m_SmoothingOut );
        }

        class VcamExtraState
        {
            public float m_previousDisplacement;
        };


        public float DesiredDistance { get; private set; }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime )
        {
            if( stage != CinemachineCore.Stage.Body )
            {
                return;
            }

            VcamExtraState extra = GetExtraState<VcamExtraState>( vcam );

            Vector3 cameraPos = state.CorrectedPosition;
            Vector3 lookAtPos = Avatar.position + Vector3.up * 1.5f;
            Vector3 direction = cameraPos - lookAtPos;
            DesiredDistance = direction.magnitude;
            RaycastHit hitInfo = new RaycastHit();
            Ray ray = new Ray( lookAtPos, direction );

            float targetDistance = direction.magnitude;

            if( Physics.SphereCast( ray, m_CameraRadius, out hitInfo, direction.magnitude, m_CollideAgainst.value ) )
            {
                targetDistance = hitInfo.distance;
            }

            if( extra.m_previousDisplacement > targetDistance )
            {
                extra.m_previousDisplacement = SpaceHub.Conference.Helpers.Damp( extra.m_previousDisplacement, targetDistance, m_SmoothingIn );
            }
            else
            {
                extra.m_previousDisplacement = SpaceHub.Conference.Helpers.Damp( extra.m_previousDisplacement, targetDistance, m_SmoothingOut );
            }
            state.PositionCorrection = ( lookAtPos + direction.normalized * extra.m_previousDisplacement ) - cameraPos;

        }

    }
}
