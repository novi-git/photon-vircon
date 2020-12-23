using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace SpaceHub.Conference
{

    public class CameraTopDown : MonoBehaviour
    {
        public float ZoomSpeed;
        public Vector2 ZoomRange;
        public CinemachineVirtualCamera VirtualCamera;
        CinemachineFramingTransposer m_Transposer;

        float m_CurrentCameraDistance;
        float m_CameraDistanceTarget;

        private void Awake()
        {
            m_Transposer = VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        private void Update()
        {
            UpdateCameraDistance();

        }

        void UpdateCameraDistance()
        {
            m_CameraDistanceTarget -= Input.GetAxis( "Mouse ScrollWheel" ) * ZoomSpeed;
            m_CameraDistanceTarget = Mathf.Clamp01( m_CameraDistanceTarget );

            m_CurrentCameraDistance = Helpers.Damp( m_CurrentCameraDistance, m_CameraDistanceTarget, 3f );

            m_Transposer.m_CameraDistance = Mathf.Lerp( ZoomRange.x, ZoomRange.y, m_CurrentCameraDistance );
        }
    }
}
