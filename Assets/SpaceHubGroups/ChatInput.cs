using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceHub.Groups
{
    public class ChatInput : MonoBehaviour
    {
        public TMP_InputField InputField;
        public CanvasGroup CanvasGroup;
        public TMPro.TextMeshProUGUI PlaceholderText;

        bool IsSelected { get { return EventSystem.current.currentSelectedGameObject == InputField.gameObject; } }

        public bool ChatIsActive { get; private set; }

        public bool ChatEnabledOverride = true;

        GroupsManager m_Manager;
        private void Awake()
        {
            m_Manager = GetComponent<GroupsManager>();
        }

        void Start()
        {
            InputField.onDeselect.AddListener( OnDeselect );
            InputField.onSelect.AddListener( OnSelect );
            UpdateSelectionStatus( false );

            // ConferenceRoomManager.Instance.SceneChangedCallback += OnSceneChanged;
        }
        /*
        void OnSceneChanged( string oldRoom, string newRoom )
        {
            Debug.Log( "Chat scene changed" );
            if ( ConferenceSceneSettings.Instance.ChatInputEnabled )
            {
                Debug.Log( "Open" );
                base.DoOpen();
            }
            else
            {
                Debug.Log( "Close" );
                base.DoClose();
            }
            UpdateSelectionStatus( false );
        }
        */
        void OnDeselect( string value )
        {
            UpdateSelectionStatus( false );
        }
        void OnSelect( string value )
        {
            UpdateSelectionStatus( true );
        }

        void UpdateSelectionStatus( bool selected )
        {
            ChatIsActive = selected;
            if( selected )
            {
                PlaceholderText.text =/* ConferenceSceneSettings.Instance.ChatInputPromptActive;//*/ "Type your message here...";
                CanvasGroup.alpha = 1f;
            }
            else
            {
                CanvasGroup.alpha = 0.25f;
                PlaceholderText.text = /*ConferenceSceneSettings.Instance.ChatInputPromptInactive;//*/ "Press Enter to start typing...";
            }
        }

        public void SendChatMessage()
        {
            if( !string.IsNullOrWhiteSpace( InputField.text ) )
            {
                m_Manager.ChannelManager.SendMessageToCurrentChannel( InputField.text );
                //PlayerLocal.Instance.SendChatMessage( ChatInput.text );
                InputField.text = "";
            }
        }

        void Update()
        {
            InputField.interactable = ChatEnabledOverride;

            if( Input.GetKeyDown( KeyCode.Return ) && ChatEnabledOverride )
            {
                if( IsSelected )
                {
                    SendChatMessage();
                    EventSystem.current.SetSelectedGameObject( null );
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject( InputField.gameObject );
                }
            }
        }
    }
}