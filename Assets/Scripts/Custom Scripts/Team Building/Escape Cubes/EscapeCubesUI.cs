using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamBuilding.Games.EscapeCubes.UI {
    public class EscapeCubesUI : MonoBehaviour {
        public Image imageObject;
        public Button imageButton;
        public static EscapeCubesUI instance;

        public BoxColor itemPickup;
        

        private void Start() {
            instance = this;
            DropItem();
        }

        public void PickupItem(BoxColor color) {
            imageButton.enabled = true;
            imageObject.color = ColorSwap(color);
            itemPickup = color; 
        }

        public void DropItem() {
            imageObject.color = ColorSwap(BoxColor.None);
            imageButton.enabled = false;
            itemPickup = BoxColor.None;
        }

        private Color ColorSwap(BoxColor color) {
            switch (color) {
                case BoxColor.Blue:
                    return Color.blue;
                case BoxColor.Green:
                    return Color.green;
                case BoxColor.Red:
                    return Color.red;
                case BoxColor.Yellow:
                    return Color.yellow;
                default:
                    return new Color(0, 0, 0, 0);
            }
        }
    }
}
