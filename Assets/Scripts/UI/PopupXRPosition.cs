using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    [RequireComponent( typeof( PopupBase))]
    public class PopupXRPosition : MonoBehaviour
    {
        public float OpenAtDistance = 1.2f;
        public float OpenAtAngle = -40f;
        public bool OnUpdate;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<PopupBase>().OpenCallback += OnOpen;
        }

        private void Update()
        {
            if( OnUpdate == true )
            {
                OnOpen();
            }
        }

        void OnOpen()
        {
            if( ViewModeManager.Instance.CurrentViewMode.GetViewMode() == ViewModeManager.ViewMode.XR )
            {
                var cam = ViewModeManager.Instance.CurrentViewMode.MainCamera.transform;

                Vector3 forward = cam.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                forward = Quaternion.Euler( 0f, OpenAtAngle, 0f ) * forward;
                transform.position = cam.transform.position + forward * OpenAtDistance;
                transform.rotation = Quaternion.LookRotation( forward );
            }
        }
    }
}