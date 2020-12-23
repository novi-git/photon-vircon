using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class InputHelpUi : MonoBehaviour
    {
        public GameObject MoveObject;
        public GameObject LookObject;
        public GameObject CameraToggleObject;
        public GameObject CameraToggleObject360;

        void Start()
        {
            ConferenceRoomManager.Instance.SceneChangedCallback += OnSceneChanged;
        }
        void OnSceneChanged( string oldScene, string newScene )
        {
            MoveObject.SetActive( ConferenceSceneSettings.Instance.IsTeleportEnabled() );
            LookObject.SetActive( ConferenceSceneSettings.Instance.IsLookEnabled() );
            CameraToggleObject.SetActive( ConferenceSceneSettings.Instance.EnableCameraToggle );
            CameraToggleObject360.SetActive( ConferenceSceneSettings.Instance.EnableCameraToggle );
        }
    }
}
