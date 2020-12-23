using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace SpaceHub.Conference
{

    public class BlockedUserListItem : MonoBehaviour
    {
        public Button UnlbockButton;
        public TextMeshProUGUI Text;

        string m_UserId;
        string m_Nickname;
        string m_ChatUser;

        private void Start()
        {
            UnlbockButton.onClick.AddListener( OnClicked );
        }

        public void SetData( string chatUser )
        {
            m_ChatUser = chatUser;
            m_UserId = Groups.Helper.GetIdFromSenderId( chatUser );
            m_Nickname = Groups.Helper.GetNicknameFromSenderid( chatUser );

            Text.text = m_Nickname;
        }

        void OnClicked()
        {
            var permissions = PlayerLocal.Instance.ChatClient.GroupsManager.Permissions;
            permissions.UnblockUser( m_ChatUser );

            var realtimeId = Groups.Helper.GetIdFromSenderId( m_ChatUser );
            var playerPair = PlayerLocal.Instance.Client.CurrentRoom.Players.Where( item => item.Value.UserId == realtimeId ).FirstOrDefault();
            if( playerPair.Value != null )
            {
                AvatarManager.Instance.OnPlayerEnteredRoom( playerPair.Value );
            }

            GameObject.Destroy( gameObject );
        }
    }
}
