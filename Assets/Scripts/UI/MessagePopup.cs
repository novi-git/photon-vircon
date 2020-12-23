using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class MessagePopup : PopupBase
    {
        public static MessagePopup Instance;

        public GameObject MessagePrefab;
        public float MessageDuration = 5f;

        public static void Show( string message, LogType logType = LogType.Log )
        {
            if( Instance != null &&
                Instance.Content != null &&
                Instance.Content.childCount < 5 )
            {
                Instance.DoShow( message, logType );
            }
        }

        public static void ShowConfirm( string message, string ok, string cancel, UnityAction onConfirm, UnityAction onCancel )
        {
            if( Instance != null &&
                Instance.Content != null &&
                Instance.Content.childCount < 5 )
            {
                Instance.DoShowConfirm( message, ok, cancel, onConfirm, onCancel );
            }
        }

        public static void ShowConfirmWithInput( string message, string prompt, string ok, string cancel, UnityAction<string> onConfirm, UnityAction onCancel )
        {
            if( Instance != null &&
                            Instance.Content != null &&
                            Instance.Content.childCount < 5 )
            {
                Instance.DoShowConfirmWithInput( message, prompt, ok, cancel, onConfirm, onCancel );
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public void OnMessageDestroyed()
        {
            if( Instance.Content.childCount == 0 )
            {
                DoClose();
            }
        }

        private void DoShow( string message, LogType logType = LogType.Log )
        {
            MessagePopupMessage newMessage = CreateMessagePopupMessage();

            DoOpen();
            newMessage.SetMessage( message, logType );
            newMessage.Show( MessageDuration );

        }

        private void DoShowConfirm( string message, string ok, string cancel, UnityAction onConfirm, UnityAction onCancel )
        {
            MessagePopupMessage newMessage = CreateMessagePopupMessage();

            DoOpen();
            newMessage.SetConfirm( message, ok, cancel, onConfirm, onCancel );
            newMessage.Show( -1 );

        }

        private void DoShowConfirmWithInput( string message, string prompt, string ok, string cancel, UnityAction<string> onConfirm, UnityAction onCancel )
        {
            MessagePopupMessage newMessage = CreateMessagePopupMessage();

            DoOpen();
            newMessage.SetConfirmWithInput( message, prompt, ok, cancel, onConfirm, onCancel );
            newMessage.Show( -1 );

        }

        MessagePopupMessage CreateMessagePopupMessage()
        {
            GameObject messageObject = Instantiate( MessagePrefab, Content );
            MessagePopupMessage newMessage = messageObject.GetComponent<MessagePopupMessage>();
            newMessage.transform.SetAsFirstSibling();

            return newMessage;
        }

        private void Update()
        {
            /*if( Input.GetKeyDown( KeyCode.Keypad1 ) )
            {
                MessagePopup.Show( "Info message", LogType.Log );
            }

            if( Input.GetKeyDown( KeyCode.Keypad2 ) )
            {
                MessagePopup.Show( "Warning message", LogType.Warning );
            }

            if( Input.GetKeyDown( KeyCode.Keypad3 ) )
            {
                MessagePopup.Show( "Error message", LogType.Error );
            } */
        }
    }
}