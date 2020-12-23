using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Groups
{
    public class InvitesManager : MonoBehaviour
    {
        public RectTransform Root;
        public TextMeshProUGUI Text;
        public AnimationCurve ShowHideCurve;

        class Invite
        {
            public string UserName;
            public string ChannelName;
            public string SenderId;
            public bool IsPrivate;
        }

        GroupsManager m_Manager;
        Queue<Invite> m_Invites = new Queue<Invite>();
        Invite m_CurrentInvite;
        private void Awake()
        {
            m_Manager = GetComponent<GroupsManager>();
            Root.anchoredPosition = Vector2.down * Root.rect.height;
        }

        public void AddInvite( string senderId, string channelName, bool isPrivate )
        {
            m_Invites.Enqueue( new Invite() { UserName = Helper.GetNicknameFromSenderid( senderId ), SenderId = senderId, ChannelName = channelName, IsPrivate = isPrivate } );
        }

        private void Update()
        {
            if( m_CurrentInvite == null && m_Invites.Count > 0 )
            {
                m_CurrentInvite = m_Invites.Dequeue();
                StartCoroutine( ShowInvite() );
            }

#if UNITY_EDITOR
            if( Input.GetKeyDown( KeyCode.I ) )
            {
                AddInvite( "23#TestName", "testChannel", true );
            }
#endif
        }

        public void OnAcceptClicked()
        {
            if( m_CurrentInvite == null )
            {
                return;
            }

            if ( m_CurrentInvite.IsPrivate )
            {
                m_Manager.ChannelManager.JoinPrivateChannel( m_CurrentInvite.SenderId );
            }
            else
            {
                m_Manager.ChannelManager.JoinChannel( m_CurrentInvite.ChannelName, true );
            }

            StartCoroutine( HideInvite() );
        }

        public void OnDeclineClicked()
        {
            m_CurrentInvite = null;
            StartCoroutine( HideInvite() );
        }

        IEnumerator ShowInvite()
        {
            if( m_CurrentInvite.IsPrivate )
            {
                Text.text = m_CurrentInvite.UserName + " invites you to chat with them! (private chat)";
            }
            else
            {
                Text.text = "New Group invite from " + m_CurrentInvite.UserName + "!";
            }
            yield return StartCoroutine( MoveRoutine( Vector2.zero, 0.5f ) );
            yield return null;
        }

        IEnumerator HideInvite()
        {
            yield return StartCoroutine( MoveRoutine( Vector2.down * Root.rect.height, 0.25f ) );

            m_CurrentInvite = null;
        }

        IEnumerator MoveRoutine( Vector2 target, float duration )
        {
            Vector2 start = Root.anchoredPosition;
            if( duration > 0f )
            {
                float timer = 0f;
                float timeSpeed = 1f / duration;
                while( timer <= 1f )
                {
                    timer += Time.deltaTime * timeSpeed;
                    Root.anchoredPosition = Vector2.Lerp( start, target, ShowHideCurve.Evaluate( timer ) );
                    yield return null;
                }
            }
            Root.anchoredPosition = target;
        }

    }
}
