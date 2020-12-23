using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SpaceHub.Conference
{

    public class EmotesUi : MonoBehaviour
    {
        public GameObject ListObject;
        public Transform ListParent;
        public GameObject ButtonPrefab;
        public bool isShowText = false;

        private void Start()
        {
            var list = CustomizationData.Instance.GetAllEmotes();
            for( int i = 0; i < list.Count; ++i )
            {
                var item = list[ i ];
                var go = Instantiate( ButtonPrefab, ListParent );
                var icon = go.transform.Find("Icon").GetComponent<Image>();
                var text = go.GetComponentInChildren<TextMeshProUGUI>();

                if(isShowText){ 
                    text.text = item.ButtonText;
                } else{
                    text.text = ""; // dont show text
                }
                
                icon.sprite = item.Icon;
                var button = go.GetComponent<Button>();

                int index = i;
                button.onClick.AddListener( () =>
                {
                    PlayEmote( index );
                } );
            }

            ListObject.SetActive( true );
        }

        public void ToggleListView()
        {
            //ListObject.SetActive( !ListObject.activeSelf );
        }

        public void PlayEmote( int index )
        {
            ConferenceChatListener.Instance.SendEmoteMessageToCurrentChannel( (byte)index, byte.MaxValue );
            //ListObject?.SetActive( false );
        }
    }
}
