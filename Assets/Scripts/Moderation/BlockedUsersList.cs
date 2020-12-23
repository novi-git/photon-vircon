using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace SpaceHub.Conference
{
    public class BlockedUsersList : MonoBehaviour
    {
        public GameObject BlockedUserPrefab;
        public RectTransform Content;

        private void OnEnable()
        {
            var permissions = PlayerLocal.Instance.ChatClient.GroupsManager.Permissions;
            var blockedUsers = permissions.BlockedUsers;

            Helpers.DestroyAllChildren( Content );

            foreach( var user in blockedUsers )
            {
                var go = Instantiate( BlockedUserPrefab, Content );

                var script = go.GetComponent<BlockedUserListItem>();
                if ( script )
                {
                    script.SetData( user );
                }
            }
        }
    }
}
