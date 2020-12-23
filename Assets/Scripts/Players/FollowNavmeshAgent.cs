using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class FollowNavmeshAgent : MonoBehaviour
    {
        public Transform FollowTransform;
        public float Damp = 2f;
        Vector3 m_position;
        private void Start()
        {
            m_position = transform.position;
        }
        private void LateUpdate()
        {
            m_position.x = FollowTransform.position.x;
            m_position.z = FollowTransform.position.z;
            m_position.y = Helpers.Damp( m_position.y, FollowTransform.position.y, Damp );
            transform.position = m_position;
            transform.rotation = FollowTransform.rotation;
        }
    }
}
