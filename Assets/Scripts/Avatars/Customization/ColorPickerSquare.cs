using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceHub.Conference
{
    public class ColorPickerSquare : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        public RectTransform Cursor;

        [Serializable]
        public class SliderEvent : UnityEvent<float, float> { }

        public UnityAction OnColorChanged;

        [SerializeField]
        protected float m_Value;
        public virtual float Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                Set( m_Saturation, value );
            }
        }

        [SerializeField]
        protected float m_Saturation;
        public virtual float Saturation
        {
            get
            {
                return m_Saturation;
            }
            set
            {
                Set( value, m_Value );
            }
        }

        [SerializeField]
        private SliderEvent m_OnValueChanged = new SliderEvent();
        public SliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        Image m_Image;

        protected override void Awake()
        {
            base.Awake();

            m_Image = GetComponent<Image>();
        }

        public void SetHue( float hue )
        {
            if( m_Image == null )
            {
                m_Image = GetComponent<Image>();
            }

            m_Image.color = Color.HSVToRGB( hue, 1f, 1f );
        }

        public void Set( float saturation, float value )
        {
            Set( saturation, value, true );
        }


        public void Set( float saturation, float value, bool sendCallback )
        {
            saturation = Mathf.Clamp01( saturation );
            value = Mathf.Clamp( value, 0.005f, 1f );

            // If the stepped value doesn't match the last one, it's time to update
            if( m_Value == value && m_Saturation == saturation )
                return;

            m_Value = value;
            m_Saturation = saturation;

            //UpdateVisuals();
            if( sendCallback )
            {
                m_OnValueChanged.Invoke( saturation, value );

                if( OnColorChanged != null )
                {
                    OnColorChanged();
                }
            }

            RectTransform clickRect = GetComponent<RectTransform>();

            Cursor.anchoredPosition = new Vector2( saturation * clickRect.rect.size.x, value * clickRect.rect.size.y );

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

                Set( localCursor.x, localCursor.y );
            }
        }

        public virtual void Rebuild( CanvasUpdate executing )
        {
#if UNITY_EDITOR
            if( executing == CanvasUpdate.Prelayout )
                onValueChanged.Invoke( Saturation, Value );
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
