using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceHub.Conference;
using TeamBuilding.Games.EscapeCubes.UI;

namespace TeamBuilding.Games.EscapeCubes {
    [RequireComponent(typeof(Outline))]
    public class CubeObject : MonoBehaviour {

        public int id = 0;

        

        public Transform AvatarTarget;
        public Transform AvoidanceTarget;
        public float AvoidanceRadius;
        public BoxColor color = BoxColor.None;
        public float DistanceCheck = 2f;

        

        void Start() {
            ConferenceInteractable Interactable = GetComponentInParent<ConferenceInteractable>();
            if (Interactable != null)
                Interactable.SelectExitCallback?.AddListener(OnEnter);

             
        }

       
        // happens every 10 seconds
        private void FixedUpdate() {
            
        }

        private void OnEnter() {
            if(Vector3.Distance(PlayerLocal.Instance.CurrentPosition, AvatarTarget.position) < 2f) {
                //Do something else
                Interact();
                return;
            }

            AvatarMovement.SetAdditionalAvoidance(AvoidanceTarget.position, AvoidanceRadius);

            if (ViewModeManager.Instance.CurrentViewMode.GetViewMode() == ViewModeManager.ViewMode.XR) {
                PlayerLocal.Instance.GoToAndLook(AvatarTarget, true, true);
            } else {
                PlayerLocal.Instance.GoToAndLook(AvatarTarget, false, true);
                
                
                //CurrentDisplay = this;

                //ConferenceSceneSettings.Instance?.DoDisableTeleport();
                //Interactable?.gameObject.SetActive(false);
                //InteractableBack?.gameObject.SetActive(true);

                //BackUI.Instance?.AddBackData(OnExit);
            }

            //BoothDisplayClickedCallback?.Invoke(this);
        }

        protected virtual void OnDrawGizmosSelected() {
            if (enabled == false) {
                return;
            }

            if (AvoidanceTarget != null) {
                Gizmos.DrawWireSphere(AvoidanceTarget.position, AvoidanceRadius);
            }
            /*
            if( ViewCamera != null )
            {
                Gizmos.matrix = Matrix4x4.TRS( ViewCamera.transform.position, ViewCamera.transform.rotation, Vector3.one );
                Gizmos.DrawFrustum( Vector3.zero, ViewCamera.fieldOfView, ViewCamera.farClipPlane, ViewCamera.nearClipPlane, ViewCamera.aspect );
                Gizmos.matrix = Matrix4x4.identity;
            }*/

            if (AvatarTarget != null) {
                Gizmos.DrawSphere(AvatarTarget.position + Vector3.up * 1.575f + AvatarTarget.forward * -0.125f, 0.15f);
                Gizmos.DrawSphere(AvatarTarget.position + Vector3.up * 1.25f + AvatarTarget.forward * -0.1f, 0.25f);
            }
        }

        public void Interact() {
            EscapeCubesUI.instance.PickupItem(color);
        }

    }
}
