using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class PlayerManager : MonoBehaviour
    {
        public GameObject TestPlayerPrefab;
        public int CreateNumberOfAvatarsWhenShiftIsPressed = 10;
        public int CreateNumberOfAvatarsWhenControlIsPressed = 100;
        public ConferenceConnector LocalPlayerConnector;
        public int CreateNumberOfAvatarsWhenLocalPlayerJoined = 0;

        
        private void Start()
        {
            PlayerLocal.Instance.Connector.JoinedRoomCallback += OnJoinedRoom;
            ConferenceRoomManager.Instance.SceneChangedCallback += OnRoomChanged;
        }

        void OnRoomChanged(string oldRoom, string newRoom)
        {
            if( PlayerLocal.Instance.Client.IsConnectedAndReady == true )
            {
                
            }
        }

        void OnJoinedRoom()
        {
            #if UNITY_EDITOR
                        StartCoroutine( CreateMultipleTestAvatars( CreateNumberOfAvatarsWhenLocalPlayerJoined ) );
            #endif
        }

        
        private void Update()
        {
            if( PlayerLocal.IsPlayerTyping() )
            {
                return;
            }
            /*
            if( Input.GetKeyDown( KeyCode.T ) )
            {
                if( Input.GetKey( KeyCode.LeftShift ) )
                {
                    StartCoroutine( CreateMultipleTestAvatars( CreateNumberOfAvatarsWhenShiftIsPressed ) );
                }
                else if( Input.GetKey( KeyCode.LeftControl ) )
                {
                    StartCoroutine( CreateMultipleTestAvatars( CreateNumberOfAvatarsWhenControlIsPressed ) );
                }
                else
                {
                    CreateTestAvatar();
                }
            } */
        }

        IEnumerator CreateMultipleTestAvatars( int number )
        {
            for( int i = 0; i < number; ++i )
            {
                CreateTestAvatar();
                yield return new WaitForSeconds( 0.1f );
            }
        }

        void CreateTestAvatar()
        {
            GameObject newTestAvatarObject = Instantiate( TestPlayerPrefab, new Vector3( 0, 0, 0 ), Quaternion.identity );
        }
    }
}
