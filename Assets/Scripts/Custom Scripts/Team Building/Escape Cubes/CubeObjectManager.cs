using ExitGames.Client.Photon;
using Photon.Realtime;
using SpaceHub.Conference;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeamBuilding.Games.EscapeCubes {
    public enum BoxColor {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }

    public class CubeObjectManager : MonoBehaviour, IOnEventCallback {

        public static CubeObjectManager instance;

        public List<CubeObject> cubes = new List<CubeObject>();
        public List<DropBox> dropboxes = new List<DropBox>();


        private int FrequencyUpdate;
        void Start() {
            FrequencyUpdate = 0;
            instance = this;
            StartCoroutine(Control());
        }

        private IEnumerator Control() {
            if (PlayerLocal.Instance.Connector == null) {
                yield break;
            }

            PlayerLocal.Instance.Client.AddCallbackTarget(this);
        }

        private void OnDestroy() {
            if (PlayerLocal.Instance.Connector != null &&
                PlayerLocal.Instance.Connector.Network != null &&
                PlayerLocal.Instance.Connector.Network.Client != null) {
                PlayerLocal.Instance.Connector.Network.Client.RemoveCallbackTarget(this);
            }
        }

        private void FixedUpdate() {

            if (PlayerLocal.Instance.Client.State != ClientState.Joined) return;            
            if (FrequencyUpdate % 50 == 0) {
                SendOptions options = new SendOptions { DeliveryMode = DeliveryMode.Reliable };
                object[] thingsToSend = new object[dropboxes.Count];
                for (int i = 0; i < dropboxes.Count; i++) {                    
                    thingsToSend[i] = dropboxes[i].IsEnabled;
                }

                SendEvent(TeamBuildingCodes.UpdateOthers,
                    thingsToSend,
                    new RaiseEventOptions { Receivers = ReceiverGroup.Others, InterestGroup = 0 },
                    options
                    );
                Debug.Log("Update Sent");
            }
            FrequencyUpdate++;
            if (FrequencyUpdate > 200) {
                FrequencyUpdate = 0;
            }
        }

        [ContextMenu("Setup Cubes")]
        void SetupItems() { 
            cubes = FindObjectsOfType<CubeObject>().ToList();
            dropboxes = FindObjectsOfType<DropBox>().ToList();
            for (int i = 0; i < cubes.Count; i++) {
                cubes[i].id = i;
                Debug.Log(cubes[i].id);
            }
            for(int i = 0; i < dropboxes.Count; i++) {
                dropboxes[i].id = i;
                Debug.Log(dropboxes[i].id);
            }
        }


        public void OnEvent(EventData photonEvent) {
            if (PlayerLocal.Instance == null ||
                PlayerLocal.Instance.Client == null ||
                PlayerLocal.Instance.Client.CurrentRoom == null ||
                PlayerLocal.Instance.Client.CurrentRoom.Players.ContainsKey(photonEvent.Sender) == false) {
                return;
            }
            Debug.Log($"Received code: {photonEvent.Code}");
            switch (photonEvent.Code) {
                case TeamBuildingCodes.InteractOn:
                    Debug.Log("Interact occured");
                    object[] item = (object[])photonEvent.CustomData;
                    BoxColor color = (BoxColor)item[0];
                    int boxID = (int)item[1];
                    foreach(DropBox box in dropboxes) {
                        if(box.id == boxID) {
                            box.DropItem(color, true);
                            break;
                        }
                    }
                    break;
                case TeamBuildingCodes.InteractOff:
                    break;
                case TeamBuildingCodes.UpdateOthers:
                    Debug.Log("Update Received");
                    object[] items = (object[])photonEvent.CustomData;
                    for (int i = 0; i < dropboxes.Count; i++) {
                        if(!dropboxes[i].IsEnabled)
                            dropboxes[i].IsEnabled = (bool)items[i];
                    }
                    break;
                default:
                    break;
            }
        }

        public static void SendEvent(byte TeamBuildingCodes, object customContent, RaiseEventOptions eventOptions, SendOptions options) {
            PlayerLocal.Instance.Client.OpRaiseEvent(TeamBuildingCodes, customContent, eventOptions, options);
        }
    }
}
