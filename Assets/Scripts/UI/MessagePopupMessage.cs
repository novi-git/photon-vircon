using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class MessagePopupMessage : MonoBehaviour
    {
        public static List<TMP_InputField> s_InputFields = new List<TMP_InputField>();
        public static bool IsInputFieldInUse()
        {
            foreach( var field in s_InputFields)
            {
                if ( field == null )
                {
                    continue;
                }
                if (field.isFocused )
                {
                    return true;
                }
            }

            return false;
        }


        public Sprite[] Icons;
        public Color[] IconColors;
        public Color[] TextColors;

        public TextMeshProUGUI Message;
        public SVGImage Icon;

        public GameObject CloseButton;
        public GameObject OkButton;
        public GameObject CancelButton;
        public TextMeshProUGUI OkButtonText;
        public TextMeshProUGUI CancelButtonText;

        public TMP_InputField InputField;

        CanvasGroup m_CanvasGroup;
        float m_Time;
        float m_Duration;
        UnityAction m_OnConfirmCallback;
        UnityAction<string> m_OnConfirmWithInputCallback;
        UnityAction m_OnCancelCallback;

        public float MessageInputHeight = 400f;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_CanvasGroup.alpha = 0;
        }

        private void OnDestroy()
        {
            if ( s_InputFields.Contains(InputField))
            {
                s_InputFields.Remove( InputField );
            }
        }

        private void Update()
        {
            m_Time += Time.unscaledDeltaTime;
            m_CanvasGroup.alpha = Mathf.Clamp01( m_Time * 2f );

            if( m_Duration > 0 )
            {
                m_CanvasGroup.alpha *= Mathf.Clamp01( ( m_Duration - m_Time ) * 2f );

                if( m_Time >= m_Duration )
                {
                    Hide();
                }
            }
        }

        public void Show( float duration )
        {
            m_Time = 0f;
            m_Duration = duration;
            gameObject.SetActive( true );
        }

        public void Hide()
        {
            transform.SetParent( null );
            Destroy( gameObject );
            MessagePopup.Instance.OnMessageDestroyed();
        }

        public void Confirm()
        {
            m_OnConfirmCallback?.Invoke();
            m_OnConfirmWithInputCallback?.Invoke( InputField.text );
            Hide();
        }

        public void Cancel()
        {
            m_OnCancelCallback?.Invoke();
            Hide();
        }

        public void SetConfirmWithInput( string message, string prompt, string ok, string cancel, UnityAction<string> onConfirm, UnityAction onCancel )
        {
            int index = GetIndexFromLogType( LogType.Log );

            Message.text = message;
            Message.color = TextColors[ index ];

            Icon.enabled = false;

            CloseButton.SetActive( false );
            OkButton.SetActive( true );
            CancelButton.SetActive( true );
            InputField.gameObject.SetActive( true );


            OkButtonText.text = ok;
            CancelButtonText.text = cancel;
            ( (TextMeshProUGUI)InputField.placeholder ).text = prompt;
            s_InputFields.Add( InputField );

            m_Duration = -1;

            m_OnConfirmCallback = null;
            m_OnConfirmWithInputCallback = onConfirm;
            m_OnCancelCallback = onCancel;

            Message.GetComponent<RectTransform>().offsetMax = new Vector2( -330, Message.GetComponent<RectTransform>().offsetMax.y );
            Message.GetComponent<RectTransform>().offsetMin = new Vector2( 30, Message.GetComponent<RectTransform>().offsetMin.y );
            Message.alignment = TextAlignmentOptions.TopLeft;
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, MessageInputHeight );
        }
        public void SetConfirm( string message, string ok, string cancel, UnityAction onConfirm, UnityAction onCancel )
        {
            int index = GetIndexFromLogType( LogType.Log );

            Message.text = message;
            Message.color = TextColors[ index ];

            Icon.sprite = Icons[ index ];
            Icon.color = IconColors[ index ];

            CloseButton.SetActive( false );
            OkButton.SetActive( true );
            CancelButton.SetActive( true );
            InputField.gameObject.SetActive( false );

            OkButtonText.text = ok;
            CancelButtonText.text = cancel;

            m_Duration = -1;
            m_OnConfirmCallback = onConfirm;
            m_OnCancelCallback = onCancel;

            Message.GetComponent<RectTransform>().offsetMax = new Vector2( -330, Message.GetComponent<RectTransform>().offsetMax.y );
        }

        public void SetMessage( string message, LogType logType = LogType.Log )
        {
            int index = GetIndexFromLogType( logType );

            Message.text = message;
            Message.color = TextColors[ index ];
            InputField.gameObject.SetActive( false );

            Icon.sprite = Icons[ index ];
            Icon.color = IconColors[ index ];

        }

        int GetIndexFromLogType( LogType logType )
        {
            switch( logType )
            {
            case LogType.Log:
                return 0;
            case LogType.Warning:
                return 1;
            default:
                return 2;
            }
        }

        [ContextMenu( "Test Info Message" )]
        void TestInfoMessage()
        {
            SetMessage( "This is an info message", LogType.Log );
        }

        [ContextMenu( "Test Warning Message" )]
        void TestWarningMessage()
        {
            SetMessage( "This is a warning message", LogType.Warning );
        }

        [ContextMenu( "Test Error Message" )]
        void TestErrorMessage()
        {
            SetMessage( "This is an error message", LogType.Error );
        }
    }
}