using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class OrientTowardsLocalPlayer : MonoBehaviour
    {
        public bool InverseForward;

        void Update()
        {
            if( PlayerLocal.Instance != null )
            {
                Vector3 lookAtPosition = ViewModeManager.Instance.CurrentViewMode.MainCamera.transform.position;

                lookAtPosition.y = transform.position.y;

                Vector3 forward = ( lookAtPosition - transform.position ).normalized;

                if( InverseForward == true )
                {
                    forward *= -1;
                }

                transform.rotation = Quaternion.LookRotation( forward );
            }
        }
    }
}