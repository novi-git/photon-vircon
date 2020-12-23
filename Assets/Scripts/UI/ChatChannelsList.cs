using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SpaceHub.Conference
{
    public class ChatChannelsList : MonoBehaviour
    {
        public GameObject ChannelButtonPrefab;
        public Groups.GroupsManager GroupsManager;

        public RectTransform PanelParent;
        public RectTransform AddChannelButton;
        public RectTransform PrivateChatsListButton;
        public Sprite PublicChannelSprite;
        public Sprite BubbleChannelSprite;
        public Color BubbleChannelColor;
        public Color FloorChannelColor;

        public List<string> ChannelList = new List<string>();

        public void OnAddButtonClicked()
        {
            GroupsManager.ChannelManager.CreateAndJoinChannel();
        }

        private void Start()
        {
            GroupsManager.ChannelManager.Minimizer?.SetEnabledAndVisible( false, false );
            GroupsManager.ChannelManager.Minimizer = null;

            GroupsManager.ChannelManager.ChannelListParent = PanelParent;
            GroupsManager.ChannelManager.ChannelButtonPrefab = ChannelButtonPrefab;

            GroupsManager.ChannelManager.CreateChannelButtonOverride += OnButtonCreated;
            GroupsManager.ChannelManager.DeleteChannelButtonCallback += OnButtonDelete;

            LoadChannelsFromPlayerPrefs();
        }



        private void OnDestroy()
        {
            GroupsManager.ChannelManager.DeleteChannelButtonCallback -= OnButtonDelete;

            SaveChannelsToPlayerPrefs();
        }

        void OnButtonDelete( Groups.ChannelData data )
        {
            var rect = data.Button.transform as RectTransform;
            var go = new GameObject( "spacer " + data.Name, typeof( RectTransform ) );
            var newRect = (RectTransform)go.transform;
            newRect.SetParent( rect.parent );
            newRect.SetSiblingIndex( rect.GetSiblingIndex() );

            StartCoroutine( ScaleRectTransform( newRect, rect.sizeDelta, Vector2.zero, true ) );

            if( ChannelList.Contains( data.Name ) )
            {
                ChannelList.Remove( data.Name );
            }
        }

        IEnumerator ScaleRectTransform( RectTransform rect, Vector2 startSize, Vector2 targetSize, bool deleteAfter )
        {
            float time = 0f;
            while( time <= 1f )
            {
                time += Time.deltaTime * 5f;
                rect.sizeDelta = Vector2.Lerp( startSize, targetSize, time * time );
                LayoutRebuilder.MarkLayoutForRebuild( rect );
                yield return null;
            }

            rect.sizeDelta = targetSize;

            if( deleteAfter )
            {
                Destroy( rect.gameObject );
            }
        }

        void OnButtonCreated( Groups.ChannelData data )
        {
            if( data.Name == SignalManager.SignalChannelName )
            {
                return;
            }

            data.CreateButton( ChannelButtonPrefab, PanelParent, ( Groups.ChannelData channelData ) => { GroupsManager.ChannelManager.SwitchChannel( channelData.Name, false ); } );

            var button = data.Button;
            Color color = Color.white;
            var image = button.transform.GetChild( 0 ).GetComponent<SVGImage>();
            var icon = button.PublicIcon[ 0 ].GetComponent<SVGImage>();

            var chatbubble = PlayerLocalChatbubble.Instance.GetCurrentChatBubble();

            if( data.Name == ConferenceRoomManager.Instance.CurrentRoomName )
            {
                button.transform.SetAsLastSibling();

                color = FloorChannelColor;
                var rect = button.transform as RectTransform;
                StartCoroutine( ScaleRectTransform( rect, Vector2.zero, rect.sizeDelta * 1.25f, false ) );

                icon.sprite = PublicChannelSprite;
            }
            else if( chatbubble != null &&
                 data.Name == chatbubble.GetVoiceRoomName() )
            {
                button.transform.SetSiblingIndex( button.transform.parent.childCount - 2 );

                color = BubbleChannelColor;
                var rect = button.transform as RectTransform;
                StartCoroutine( ScaleRectTransform( rect, Vector2.zero, rect.sizeDelta * 1.25f, false ) );

                icon.sprite = BubbleChannelSprite;
            }
            else
            {
                button.transform.SetAsFirstSibling();

                var rect = button.transform as RectTransform;
                StartCoroutine( ScaleRectTransform( rect, Vector2.zero, rect.sizeDelta, false ) );

                string colorcode = data.Name.Substring( data.Name.Length - 7 );
                if( ColorUtility.TryParseHtmlString( colorcode, out color ) == false )
                {
                    color = Color.HSVToRGB( Random.value, 1f, 1f );
                }

                ChannelList.Add( data.Name );
            }

            image.color = color;
            AddChannelButton?.SetAsFirstSibling();
            PrivateChatsListButton?.SetAsFirstSibling();
        }

        [System.Serializable]
        class PlayerPrefContainer
        {
            public string[] Channels;
        }

        void LoadChannelsFromPlayerPrefs()
        {
            var data = JsonUtility.FromJson<PlayerPrefContainer>( PlayerPrefs.GetString( "ChatChannels" ) );
            if ( data != null )
            {
                foreach( var channel in data.Channels)
                {
                    GroupsManager.ChannelManager.JoinChannel( channel, true );
                }
            }
        }

        void SaveChannelsToPlayerPrefs()
        {
            var data = new PlayerPrefContainer()
            {
                Channels = ChannelList.ToArray()
            };
            PlayerPrefs.SetString( "ChatChannels", JsonUtility.ToJson( data ) );
            PlayerPrefs.Save();
        }


    }
}
