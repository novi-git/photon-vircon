using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



namespace SpaceHub.Groups
{
    public class ChatTopBar : MonoBehaviour
    {

        public TextMeshProUGUI HeadlineText;
        public Button AcceptPrivateChatButton;

        public void UpdateCurrent( GroupsManager manager )
        {
            var channelManager = manager.ChannelManager;

            var channelData = channelManager.CurrentChannel;
            if( channelData == null )
            {
                gameObject.SetActive( false );
                return;
            }
            else
            {
                gameObject.SetActive( true );
            }

            HeadlineText.text = channelManager.GetCurrentChannelDisplayName( channelData.Channel );

            /*if( channelData.IsPrivate == false )// && manager.Permissions.IsPrivateChatConfirmed( channelData.Name ) == false )
            {
                AcceptPrivateChatButton.gameObject.SetActive( false );
                return;
            }*/


            bool showAccept = channelData.Channel != null && channelData.IsPrivate && manager.Permissions.IsPrivateChatConfirmed( channelData.Name ) == false;
            AcceptPrivateChatButton.gameObject.SetActive( showAccept );





        }
    }
}
