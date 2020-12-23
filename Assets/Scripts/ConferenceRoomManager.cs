using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace SpaceHub.Conference
{


    public class ConferenceRoomManager : MonoBehaviour
    {
        public static ConferenceRoomManager Instance;

        static string m_RoomToLoad;
        static string m_PreviousRoom;
        static string m_SpawnPointId;
        const string ExpoManagerScene = "Expo";
        
        public string DefaultExpoRoom = "ExpoRoomMain";

        public UnityAction<string, string> SceneChangedCallback;

        Scene m_CurrentRoomScene;

        public string CurrentRoomName { get { return m_RoomToLoad; } }

        private void Awake()
        {
            Instance = this;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            if( string.IsNullOrEmpty( m_RoomToLoad ) )
            {
                m_RoomToLoad = DefaultExpoRoom;
            }
            if( string.IsNullOrEmpty( m_SpawnPointId ) )
            {
                m_SpawnPointId = SpawnPoint.Default;
            }
        }
        private void Start()
        {
            LoadRoom( m_RoomToLoad, m_SpawnPointId, null );
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public static void LoadRoom( string sceneName, string spawnPoint, string returnSpawnPoint, UnityAction callback = null )
        {
            Debug.Log( "Load room: " + sceneName + " - At spawnpoint: " + spawnPoint );

            ConnectingOverlay.Show();

            m_RoomToLoad = sceneName;
            m_SpawnPointId = spawnPoint;


            if( Instance == null )
            {
                SceneManager.LoadScene( ExpoManagerScene, LoadSceneMode.Single );
                return;
            }

            if ( callback != null )
            {
                Instance.StartCoroutine( Instance.WaitForLoadAndConnect( callback ) );
            }

            if( Instance.m_CurrentRoomScene.IsValid() && Instance.m_CurrentRoomScene.isLoaded )
            {
                PlayerLocal.Instance.Connector.IsMessageQueueRunning = false;

                Debug.Log( "Unloading Scene: " + Instance.m_CurrentRoomScene.name );
                m_PreviousRoom = Instance.m_CurrentRoomScene.name;

                SceneManager.UnloadSceneAsync( Instance.m_CurrentRoomScene );
            }
            else
            {
                SceneManager.LoadSceneAsync( m_RoomToLoad, LoadSceneMode.Additive );
            }

            BackUI.Instance?.RemoveLast();
            if( string.IsNullOrEmpty( returnSpawnPoint ) == false )
            {
                BackUI.Instance?.AddBackData( () => { LoadRoom( m_PreviousRoom, returnSpawnPoint, null ); }, "Back to Expo", m_PreviousRoom );
            }
        }

        IEnumerator WaitForLoadAndConnect( UnityAction callback )
        {
            while( m_RoomToLoad != m_CurrentRoomScene.name )
            {
                yield return null;
            }

            yield return null;

            while( PlayerLocal.Instance.Client.CurrentRoom == null || PlayerLocal.Instance.Client.CurrentRoom.Name != m_RoomToLoad || PlayerLocal.Instance.Client.IsConnectedAndReady == false )
            {
                yield return null;
            }

            callback?.Invoke();
        }

        public string GetCurrentRoomName()
        {
            return m_RoomToLoad;
        }

        void OnSceneUnloaded( Scene scene )
        {
            if( string.IsNullOrEmpty( m_RoomToLoad ) == false )
            {
                SceneManager.LoadSceneAsync( m_RoomToLoad, LoadSceneMode.Additive );
            }
        }

        void OnSceneLoaded( Scene scene, LoadSceneMode mode )
        {
            Debug.Log( "Loaded Scene " + scene.name + ". Mode: " + mode.ToString() );
            if( mode != LoadSceneMode.Additive )
            {
                return;
            }


            m_CurrentRoomScene = scene;
            SceneManager.SetActiveScene( m_CurrentRoomScene );
            LightProbes.Tetrahedralize();

            var spawnPoint = SpawnPoint.GetSpawnPositionById( m_SpawnPointId );

            PlayerLocal.Instance.GoToAndLook( spawnPoint.transform, true, true );

            Instance.SceneChangedCallback?.Invoke( m_PreviousRoom, m_RoomToLoad );

            StartCoroutine( JoinRoomRoutine( m_RoomToLoad ) );

            PlayerLocal.Instance.Connector.IsMessageQueueRunning = true;
        }

        IEnumerator JoinRoomRoutine( string roomName )
        {
            Debug.Log( "Join Room Routine. Is Connected and ready: " + PlayerLocal.Instance.Client.IsConnectedAndReady );

            while( PlayerLocal.Instance.Client.IsConnectedAndReady == false )
            {
                yield return null;
            }

            Debug.Log( "Join room " +  roomName);

            PlayerLocal.Instance.Connector.JoinOrChangeRoom( roomName );
        }

    }
}
