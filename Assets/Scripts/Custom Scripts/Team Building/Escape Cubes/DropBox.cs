using Photon.Realtime;
using SpaceHub.Conference;
using System.Collections;
using System.Collections.Generic;
using TeamBuilding.Games.EscapeCubes.UI;
using UnityEngine;

namespace TeamBuilding.Games.EscapeCubes {
    [RequireComponent(typeof(Outline))]
    public class DropBox : MonoBehaviour {
        public int id = 0;  
        public Transform AvatarTarget;
        public Transform AvoidanceTarget;
        public float AvoidanceRadius;
        public ParticleSystem system;
        public BoxColor currentColorRequired = BoxColor.Green;
        public float DistanceCheck = 2f;
        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value;
                system.gameObject.SetActive(value);
            } }

        private bool _isEnabled = false;


        // Start is called before the first frame update
        void Start() {
            _isEnabled = false;
            ConferenceInteractable Interactable = GetComponentInParent<ConferenceInteractable>();
            if (Interactable != null)
                Interactable.SelectExitCallback?.AddListener(OnEnter);
            system.gameObject.SetActive(false);
            
        }
        

        private void OnEnter() {
            if (Vector3.Distance(PlayerLocal.Instance.CurrentPosition, AvatarTarget.position) < 2f) {
                Debug.Log("Doing something");
                DropItem(EscapeCubesUI.instance.itemPickup, false);
                EscapeCubesUI.instance.DropItem();
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

        public void DropItem(BoxColor itemColor, bool isSend) {
            //if (isEnabled) return;
            if (this.currentColorRequired == itemColor) {
                system.gameObject.SetActive(true);
                Debug.Log("Correct Color");
                object[] items = new object[] { currentColorRequired, id };
                RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others, InterestGroup = 0 };
                if (!isSend) {
                    CubeObjectManager.SendEvent(TeamBuildingCodes.InteractOn,
                        items,
                        options,
                        new ExitGames.Client.Photon.SendOptions {
                            DeliveryMode = ExitGames.Client.Photon.DeliveryMode.Reliable
                        });
                }

                _isEnabled = true;
                return;
            }

            Debug.Log($"{itemColor.ToString()} is not the same as {currentColorRequired.ToString()}");
        }
    }
}
