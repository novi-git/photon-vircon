using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{


    public class StageAttendeeManager : MonoBehaviour
    {
        public Transform AvatarParent;

        public bool UseDebugAttendees;
        public int AttendeeCount;

        StageHandlerBase m_Handler;
        

        int m_AttendeeCount = -1;

        private void Awake()
        {
            m_Handler = transform.parent.GetComponentInChildren<StageHandlerBase>();
        }

        public int GetAttendeeCount()
        {
            if ( UseDebugAttendees)
            {
                return AttendeeCount;
            }
            return m_Handler.GetAttendeesCount();
        }

        public Transform GetRandomSpawnTransform()
        {
            int id = Random.Range( 0, m_AttendeeCount );
            return AvatarParent.GetChild( id );
        }

        private void Update()
        {
            var count = GetAttendeeCount();
            count = Mathf.Min( count, AvatarParent.childCount );

            if( m_AttendeeCount == count )
            {
                return;
            }
            m_AttendeeCount = count;

            for( int i = 0; i < AvatarParent.childCount; ++i )
            {
                AvatarParent.GetChild( i ).gameObject.SetActive( i < m_AttendeeCount );
            }
        }

    }
}