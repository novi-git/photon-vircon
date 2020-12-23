using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class CameraInteraction : MonoBehaviour
    {
        public ConferenceInteractable EnterInteractable;
        public ConferenceInteractable ExitInteractable;

        public GameObject CameraObject;

        void Awake()
        {
            EnterInteractable.SelectExitCallback.AddListener( OnEnter );
            ExitInteractable.SelectExitCallback.AddListener( OnExit );

            SetEntered( false );
        }

        void SetEntered( bool entered )
        {
            EnterInteractable.gameObject.SetActive( !entered );
            ExitInteractable.gameObject.SetActive( entered );
            CameraObject.SetActive( entered );
        }

        void OnEnter()
        {
            SetEntered( true );
        }

        void OnExit()
        {
            SetEntered( false );
        }
    }
}
