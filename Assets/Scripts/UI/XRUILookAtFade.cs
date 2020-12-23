using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    [RequireComponent( typeof( CanvasGroup))]
    public class XRUILookAtFade : MonoBehaviour
    {
        CanvasGroup m_CanvasGroup;

        Transform m_CameraTransform;
        Transform m_Transform;

        // Start is called before the first frame update
        void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_Transform = transform;
        }

        private void Start()
        {
            if( ViewModeManager.Instance == null )
            {
                Debug.LogWarning( "ViewModeManager not found. Can't find camera for XRUILookAtFade" );
                DestroyImmediate( this );
                return;
            }

            m_CameraTransform = ViewModeManager.Instance.CurrentViewMode.MainCamera.transform;
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            if( m_CameraTransform == null )
            {
                return;
            }

            m_CanvasGroup.alpha = Mathf.Clamp01( Vector3.Dot( m_CameraTransform.forward, m_Transform.forward ) );
        }
    }
}