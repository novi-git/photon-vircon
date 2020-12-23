using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class UIFunnelScreen : MonoBehaviour
    {
        Vector3 m_DefaultPosition;
        public float ScrollOffset = 100f;
        public float FadeoutSpeed = 4f;
        public float FadeinSpeed = 2f;

        public Transform m_Target;
        public CanvasGroup m_CanvasGroup;

        public bool StartVisible;

        float m_DirectionFactor = 1f;

        public bool IsVisible { get; private set; }

        public UnityAction ShowCallback;
        public UnityAction HideCallback;


        void Awake()
        {
            m_DefaultPosition = transform.localPosition;
            m_CanvasGroup = GetComponent<CanvasGroup>();

            m_CanvasGroup.blocksRaycasts = false;
            m_CanvasGroup.alpha = 0f;
            IsVisible = StartVisible;

            if( StartVisible )
            {
                Show();
            }
        }

        

        public void SetVisible( bool value, bool isForward = true )
        {
            m_DirectionFactor = isForward ? 1f : -1f;
            if( IsVisible != value )
            {
                if( value )
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        [ContextMenu( "Show" )]
        public void Show()
        {
            ShowCallback?.Invoke();
            StopAllCoroutines();
            IsVisible = true;
            StartCoroutine( ShowRoutine() );
        }

        public void FadeIn()
        {
            ShowCallback?.Invoke();
            StopAllCoroutines();
            IsVisible = true;
            m_DirectionFactor = 0f;
            StartCoroutine( ShowRoutine( 0f ) );
        }

        [ContextMenu( "Hide" )]
        public void Hide()
        {
            HideCallback?.Invoke();
            StopAllCoroutines();
            IsVisible = false;
            StartCoroutine( HideRoutine() );
        }

        IEnumerator HideRoutine()
        {

            m_CanvasGroup.blocksRaycasts = false;
            float t = 0f;
            while( t < 1f )
            {
                t += Time.deltaTime * FadeoutSpeed;
                float factor = ( t * t );
                transform.localPosition = m_DefaultPosition + Vector3.left * factor * ScrollOffset * m_DirectionFactor;
                if( m_CanvasGroup != null )
                {
                    m_CanvasGroup.alpha = 1f - t;
                }
                yield return null;
            }
        }

        IEnumerator ShowRoutine( float delay = 0.3f )
        {
            if( delay > 0f )
            {
                yield return new WaitForSeconds( delay );
            }

            m_CanvasGroup.blocksRaycasts = true;
            float t = 0f;
            while( t < 1f )
            {
                t += Time.deltaTime * FadeinSpeed;
                float temp = t - 1;
                float factor = ( temp * temp );
                transform.localPosition = m_DefaultPosition + Vector3.right * factor * ScrollOffset * m_DirectionFactor;

                if( m_CanvasGroup != null )
                {
                    m_CanvasGroup.alpha = t;
                }
                yield return null;
            }


            transform.localPosition = m_DefaultPosition;
        }

    }
}