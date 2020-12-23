using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace SpaceHub.Groups
{

    public class VoiceControl : MonoBehaviour
    {
        public delegate void ConfirmLeaveCallDelegate( UnityAction onSuccess );

        GroupsManager m_Manager;
        VoiceManager m_VoiceManager;

        public Image BackgroundImage;
        public Color BackgroundColorInCall;
        public Color BackgroundColorWaiting;
        public Color BackgroundColorOffline;

        public MinimizeElement Minimizer;
        public TextMeshProUGUI StatusText;

        public GameObject JoinCallButton;
        public GameObject LeaveCallButton;

        public RectTransform Spacer;
        public float SpacerSize = 15f;
        public float SpacerScaleSpeed = 5f;

        public ConfirmLeaveCallDelegate ConfirmLeaveCall = ( UnityAction onSuccess ) => { onSuccess?.Invoke(); };

        string m_CurrentRoom;

        public bool HideControllsOverride;

        private void Awake()
        {
            m_Manager = GetComponent<GroupsManager>();
            m_VoiceManager = GetComponent<VoiceManager>();
            UpdateRoomStatusText();
        }

        private void Update()
        {
            bool hidden = m_VoiceManager.VoiceConnection == null || m_VoiceManager.VoiceConnection.Client == null;
            hidden |= HideControllsOverride;

            Minimizer?.SetEnabledAndVisible( !hidden, true );
            if( hidden )
            {
                return;
            }

            //StatusText.text = m_Manager.VoiceConnection.ClientState.ToString();

            string newRoom = null;
            var room = m_VoiceManager.VoiceConnection.Client.CurrentRoom;
            if( room != null )
            {
                newRoom = room.Name;
            }

            if( newRoom != m_CurrentRoom )
            {
                m_CurrentRoom = newRoom;
                UpdateRoomStatusText();
            }



            UpdateCallButtons();
            UpdateSpacer();
        }

        void UpdateSpacer()
        {
            if( Spacer == null )
            {
                return;
            }

            float target = IsSameChatChannelAsVoiceRoomSelected() ? 0f : SpacerSize;
            Spacer.sizeDelta = Vector2.MoveTowards( Spacer.sizeDelta, new Vector2( 1f, target ), Time.deltaTime * SpacerScaleSpeed );

            LayoutRebuilder.MarkLayoutForRebuild( Spacer );

        }

        bool IsSameChatChannelAsVoiceRoomSelected()
        {
            var channel = m_Manager.ChannelManager.GetCurrentChannelName();
            if( string.IsNullOrEmpty( channel ) == false )
            {
                var data = m_Manager.ChannelManager.GetChannelData( channel );

                if( data != null )
                {
                    return m_CurrentRoom == data.VoiceRoomName || string.IsNullOrEmpty( m_CurrentRoom );
                }
            }
            return false;
        }

        void UpdateCallButtons()
        {
            bool isInCall = m_VoiceManager.VoiceConnection.Client.CurrentRoom != null;
            bool canStartCall = m_VoiceManager.VoiceConnection.Client.IsConnectedAndReady;
            JoinCallButton?.SetActive( isInCall == false && canStartCall );
            LeaveCallButton?.SetActive( isInCall );

            Color col = BackgroundColorOffline;
            if( isInCall )
            {
                col = m_VoiceManager.VoiceConnection.Client.CurrentRoom.PlayerCount > 1 ? BackgroundColorInCall : BackgroundColorWaiting;
            }

            BackgroundImage.color = col;
        }

        void UpdateRoomStatusText()
        {
            if( m_CurrentRoom == null )
            {
                if( m_VoiceManager.VoiceConnection == null )
                {
                    StatusText.text = "Disconnected.";
                }
                else
                {
                    StatusText.text = "Not in Voicechat.";
                }
            }
            else
            {
                int count = m_VoiceManager.VoiceConnection.Client.CurrentRoom.PlayerCount;

                if( count <= 1 )
                {
                    StatusText.text = "Waiting for others.";
                }
                else
                {
                    StatusText.text = "Chatting with " + count + " others.";
                }
            }
        }

        public void OnJoinCallClicked()
        {
            var channel = m_Manager.ChannelManager.CurrentChannel;
            if( channel != null )
            {
                if( channel.IsPrivate == false )
                {
                    m_VoiceManager.JoinRoom( channel.Name );
                }
                else
                {
                    var data = m_Manager.ChannelManager.GetChannelData( channel.Name );
                    if( data != null )
                    {
                        m_VoiceManager.JoinRoom( data.VoiceRoomName );
                    }
                }
            }
        }

        public void OnLeaveCallClicked()
        {
            ConfirmLeaveCall?.Invoke( DoLeaveCall );
        }

        void DoLeaveCall()
        {
            m_VoiceManager.LeaveRoom();
        }
    }
}
