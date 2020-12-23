using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SpaceHub.SimpleJSON;


namespace SpaceHub.Conference
{
    public class EventsPanel : MonoBehaviour
    {
        public GameObject ButtonPrefab;
        public GameObject ClassicJoinPanel;
        public GameObject NoEventsText;

        // Start is called before the first frame update
        void Start()
        {
            if( string.IsNullOrEmpty( ConferenceServerSettings.Instance.GetApiBaseUrl() ) )
            {
                ShowClassicJoinPanel();
                return;
            }

            StartCoroutine( GetEventsRoutine() );
        }

        IEnumerator GetEventsRoutine()
        {
            yield return null;

            string url = ConferenceServerSettings.Instance.GetApiRoute( "v1/events" );
            using( var request = UnityWebRequest.Get( url ) )
            {
                yield return request.SendWebRequest();

                if( request.isNetworkError || request.isHttpError )
                {
                    Debug.LogError( request.error );
                }
                else
                {
                    Debug.Log( request.downloadHandler.text );

                    JSONNode node = JSON.Parse( request.downloadHandler.text );

                    if( node[ "Events" ].Count == 0 )
                    {
                        NoEventsText.SetActive( true );
                    }

                    foreach( var eventObject in node[ "Events" ].Children )
                    {
                        var newButtonObject = Instantiate( ButtonPrefab );
                        var newButtonTransform = newButtonObject.GetComponent<RectTransform>();
                        var newButton = newButtonObject.GetComponent<Button>();

                        newButton.GetComponentInChildren<TextMeshProUGUI>().text = "Join " + eventObject[ "name" ];

                        newButton.onClick.AddListener( delegate ()
                        {
                            CustomizationData.Instance.LoadArtPack( eventObject[ "art_pack" ] );

                            PlayerPrefs.SetString( "EventId", eventObject[ "id" ] );
                            PlayerPrefs.SetString( "EventArtPack", eventObject[ "art_pack" ] );
                            PlayerPrefs.SetString( "PhotonRegion", eventObject[ "server_region" ] );

                            SceneManager.LoadScene( "CustomizationRoom" );
                        } );

                        newButtonTransform.SetParent( transform );
                        newButtonTransform.localPosition = Vector3.zero;
                        newButtonTransform.localRotation = Quaternion.identity;
                        newButtonTransform.localScale = Vector3.one;
                    }
                }
            }
        }

        void ShowClassicJoinPanel()
        {
            ClassicJoinPanel.SetActive( true );
            gameObject.SetActive( false );
        }

        public void JoinWithoutEvent()
        {
            PlayerPrefs.SetString( "EventId", "" );
            SceneManager.LoadScene( "CustomizationRoom" );
            PlayerPrefs.SetString("PhotonRegion", "jp");
        }

    }
}