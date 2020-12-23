using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceHub.Conference
{

    public class GoToStageInteractable : MonoBehaviour
    {
        public bool SpeakerEntrance;
        public string StageSceneName;

        public string SpawnPointId;
        public string ReturnSpawnPointId;

        private void Awake()
        {
            ConferenceInteractable interactable = GetComponentInChildren<ConferenceInteractable>();

            if( interactable != null )
            {
                interactable.SelectExitCallback?.AddListener( GotoStage );
            }
        }

        Vector3 GetMovementPosition()
        {
            return transform.position + transform.forward;
        }

        public void GotoStage()
        {
            SpeakerHandler.IsSpeaker = SpeakerEntrance;

            PlayerLocal.Instance.SendEnterPortal( GetMovementPosition() );
            ConferenceRoomManager.LoadRoom( StageSceneName, SpawnPointId, ReturnSpawnPointId );
        }
    }
}
