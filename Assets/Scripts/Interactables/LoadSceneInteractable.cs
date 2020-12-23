using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceHub.Conference
{
    public class LoadSceneInteractable : MonoBehaviour
    {
        public string SceneName;
        public string SpawnPointId;
        public string ReturnSpawnPointId;

        private void Awake()
        {
            GetComponentInChildren<ConferenceInteractable>().SelectExitCallback?.AddListener( LoadScene );
        }

        Vector3 GetMovementPosition()
        {
            return transform.position + transform.forward;
        }

        void LoadScene( )
        {
            PlayerLocal.Instance.SendEnterPortal( GetMovementPosition() );
            ConferenceRoomManager.LoadRoom( SceneName, SpawnPointId, ReturnSpawnPointId );
        }
    }
}
