using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class AvatarBodyDynamics : MonoBehaviour
    {
        Vector3 m_BodyPosition;
        Vector3 m_BodyVelocity;

        Vector3 m_BodyDownPosition;
        Vector3 m_BodyDownVelocity;

        Vector3 m_BodyRightDirection;
        Vector3 m_BodyRightVelocity;

        float m_HeadToBodyUpOffset;
        float m_HeadToBodyForwardOffset;
        float neckAngle = 90f;

        public float BodyFollowSpeed = 0.25f;
        public float BodyRotationSpeed = 0.25f;
        public float BodyRotationSpeedY = 0.25f; 
        

        public Transform HeadPivot;
        public Transform BoneNeck;
        public Transform BoneTorso;

        public Transform Eyes;


        private void Start()
        {
            StartCoroutine( EyeBlinkRoutine() );
        }

        IEnumerator EyeBlinkRoutine()
        {
            if ( Eyes == null )
            {
                yield break;
            }
            Vector3 defaultScale = Eyes.localScale;
            Vector3 blinkScale = defaultScale;
            blinkScale.y *= 0.15f;

            var blinkWait = new WaitForSeconds( 0.125f );
            while( true )
            {
                yield return new WaitForSeconds( Random.Range( 3f, 4f ) );

                Eyes.localScale = blinkScale;
                yield return blinkWait;
                Eyes.localScale = defaultScale;
            }
        }

        private void Update()
        {
            UpdateBody();
        }

        private void Awake()
        {
            var offset = HeadPivot.InverseTransformDirection( BoneTorso.position - HeadPivot.position );
            m_HeadToBodyForwardOffset = offset.x;
            m_HeadToBodyUpOffset = offset.y;
        }

        public void UpdateBody()
        {
            Vector3 targetTorsoPosition = Vector3.up * m_HeadToBodyUpOffset + HeadPivot.forward * m_HeadToBodyForwardOffset + HeadPivot.position;
            Vector3 torsoMovement = targetTorsoPosition - m_BodyPosition;

            if( torsoMovement.magnitude > 0.1f )
            {
                m_BodyPosition = targetTorsoPosition - torsoMovement.normalized * 0.1f;
            }

            BoneTorso.position = Vector3.SmoothDamp( m_BodyPosition, targetTorsoPosition, ref m_BodyVelocity, BodyFollowSpeed );
            m_BodyPosition = BoneTorso.position;
            Vector3 neckForward = HeadPivot.forward;

            BoneNeck.LookAt( m_BodyPosition, neckForward );
            BoneNeck.Rotate( Vector3.right, neckAngle, UnityEngine.Space.Self ); 

            Vector3 bodyDownTargetPosition = HeadPivot.position + Vector3.down * 0.5f;

            if( Vector3.Distance( m_BodyDownPosition, bodyDownTargetPosition ) > 1f )
            {
                m_BodyDownPosition = bodyDownTargetPosition + ( m_BodyDownPosition - bodyDownTargetPosition ).normalized;
            }

            m_BodyDownPosition = Vector3.SmoothDamp( m_BodyDownPosition, bodyDownTargetPosition, ref m_BodyDownVelocity, BodyRotationSpeed );
            m_BodyRightDirection = Vector3.SmoothDamp( m_BodyRightDirection, HeadPivot.right, ref m_BodyRightVelocity, BodyRotationSpeedY );

            Quaternion targetRotation = Quaternion.LookRotation( -Vector3.Cross( m_BodyRightDirection, Vector3.up ), m_BodyDownPosition - HeadPivot.position );
            BoneTorso.rotation = targetRotation;
        }
    }
}