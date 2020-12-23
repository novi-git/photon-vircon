using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SpaceHub.Conference
{

    public class CustomizationRotator : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        Vector2 m_DragStart;


        public Transform AvatarParent;
        public float RotationSpeed = 0.1f;
        public float ResetDamp = 1f;
        bool m_IsDragging;

        public void OnBeginDrag( PointerEventData eventData )
        {
            m_DragStart = eventData.position;
            m_IsDragging = true;
        }

        public void OnEndDrag( PointerEventData eventData )
        {
            m_IsDragging = false;
        }
        public void OnDrag( PointerEventData eventData )
        {
            AvatarParent.Rotate( Vector3.up, eventData.delta.x * RotationSpeed );
        }

        void Update()
        {
            if ( m_IsDragging == false )
            {
                AvatarParent.rotation = Helpers.Damp( AvatarParent.rotation, Quaternion.identity, ResetDamp );
            }
        }
    }
}
