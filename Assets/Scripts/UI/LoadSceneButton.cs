using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceHub.Conference
{

    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName;
        public string SpawnPointName;

        public void OnClick()
        {
            ConferenceRoomManager.LoadRoom( SceneName, SpawnPointName, null );
        }
    }
}
