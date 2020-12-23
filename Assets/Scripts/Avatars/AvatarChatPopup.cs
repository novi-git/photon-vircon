using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class AvatarChatPopup : MonoBehaviour
    {
        public GameObject Visuals;
        public GameObject MessagePrefab;

        public Vector3 UpOffset;
        public float UpSpeed;

        float m_HideInSeconds;

        Transform m_LastMessageTransform;

        private void Update()
        {
            if( m_HideInSeconds > 0f )
            {
                m_HideInSeconds = Mathf.MoveTowards( m_HideInSeconds, 0f, Time.deltaTime );

                if( m_HideInSeconds == 0 )
                {
                    HideMessage();
                }
            }
        }
        public void ShowMessage( string message )
        {
            var go = Instantiate( MessagePrefab, Vector3.zero, Quaternion.identity, Visuals.transform );
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            go.GetComponentInChildren<TextMeshPro>().text = message;
            
            if( m_LastMessageTransform != null )
            {
                m_LastMessageTransform.SetParent( go.transform, false );
            }

            m_LastMessageTransform = go.transform;
            StartCoroutine( GoUpRoutine( go.transform ) );
            Destroy( go, 7f );
        }

        IEnumerator GoUpRoutine( Transform trans )
        {
            float t = 1f;
            while( t > 0f )
            {
                t -= Time.deltaTime * UpSpeed;
                trans.localPosition = UpOffset * ( 1f - t * t );
                yield return null;
            }
            trans.localPosition = UpOffset;
        }

        void HideMessage()
        {
            Visuals.SetActive( false );
        }
    }
}