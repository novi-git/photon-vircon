using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class AvatarSkirtFollow : MonoBehaviour
    {
        Vector3 m_TargetPosition;
        Vector3 m_Scale = Vector3.one;

        public Vector3 TargetOffset;
        public float Speed;
        public float Damping = 2f;


        Vector3 m_Velocity;
        float m_DefaultScale;
        private void Start()
        {
            m_DefaultScale = TargetOffset.magnitude;

            m_TargetPosition = CalcTarget();
            m_Velocity = Vector3.zero;
        }

        Vector3 CalcTarget()
        {
            return transform.position + TargetOffset;
        }

        private void LateUpdate()
        {
            Vector3 delta = CalcTarget() - m_TargetPosition;
            m_Velocity = Helpers.Damp( m_Velocity, delta * Speed, 5f );
            m_Velocity = Helpers.Damp( m_Velocity, Vector3.zero, Damping );

            m_Scale.z = 1f + ( Vector3.Distance( m_TargetPosition, transform.position ) - m_DefaultScale );

            m_TargetPosition += m_Velocity * Time.deltaTime;

            transform.LookAt( m_TargetPosition, transform.parent.forward );
            transform.localScale = m_Scale;
        }
    }
}
