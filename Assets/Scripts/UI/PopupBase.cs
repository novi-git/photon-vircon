using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class PopupBase : MonoBehaviour
    {
        public RectTransform Content;

        public UnityAction OpenCallback;
        public UnityAction CloseCallback;

        protected void DoOpen()
        {
            OpenCallback?.Invoke();
            Content.gameObject.SetActive( true );
        }

        protected void DoClose()
        {
            CloseCallback?.Invoke();
            Content.gameObject.SetActive( false );
        }
        public bool IsOpen()
        {
            return Content.gameObject.activeSelf;
        }
    }
}