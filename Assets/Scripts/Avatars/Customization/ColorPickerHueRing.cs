using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceHub.Conference
{
    public class ColorPickerHueRing : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        [Serializable]
        public class SliderEvent : UnityEvent<float> { }

        [SerializeField]
        protected float m_Hue;
        public virtual float Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Set( value );
            }
        }

        public UnityAction OnColorChanged;

        [SerializeField]
        private SliderEvent m_OnValueChanged = new SliderEvent();
        public SliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        public RectTransform CursorRotateParent;

        public void Set( float input )
        {
            Set( input, true );
        }


        public void Set( float input, bool sendCallback )
        {
            // Clamp the input
            float newValue = input % 1;

            if( newValue < 0 )
            {
                newValue += 1;
            }

            // If the stepped value doesn't match the last one, it's time to update
            if( m_Hue == newValue )
                return;

            m_Hue = newValue;

            //UpdateVisuals();
            if( sendCallback )
            {
                m_OnValueChanged.Invoke( newValue );

                if( OnColorChanged != null )
                {
                    OnColorChanged();
                }
            }

            CursorRotateParent.localRotation = Quaternion.Euler( 0, 0, -m_Hue * 360f );
        }

        public virtual void OnDrag( PointerEventData eventData )
        {
            if( !MayDrag( eventData ) )
                return;
            UpdateDrag( eventData, eventData.pressEventCamera );
        }

        private bool MayDrag( PointerEventData eventData )
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        void UpdateDrag( PointerEventData eventData, Camera cam )
        {
            RectTransform clickRect = GetComponent<RectTransform>();

            if( clickRect != null )
            {
                Vector2 localCursor;
                if( !RectTransformUtility.ScreenPointToLocalPointInRectangle( clickRect, eventData.position, cam, out localCursor ) )
                    return;
                localCursor -= clickRect.rect.position;

                localCursor.x /= clickRect.rect.size.x;
                localCursor.y /= clickRect.rect.size.y;

                Vector2 direction = localCursor - new Vector2( 0.5f, 0.5f );
                direction.Normalize();

                float angle = Vector2.Angle( Vector2.up, direction );

                if( localCursor.x < 0.5f )
                {
                    angle = 360 - angle;
                }

                Set( angle / 360f );
            }
        }

        public virtual void Rebuild( CanvasUpdate executing )
        {
#if UNITY_EDITOR
            if( executing == CanvasUpdate.Prelayout )
                onValueChanged.Invoke( Hue );
#endif
        }

        public virtual void OnInitializePotentialDrag( PointerEventData eventData )
        {
            eventData.useDragThreshold = false;
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

    }
}