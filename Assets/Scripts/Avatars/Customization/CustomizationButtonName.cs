using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceHub.Conference
{
    public class CustomizationButtonName : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener( OnClick );
        }

        void OnClick()
        {
            var menu = GetComponentInParent<CustomizationMenu>();
            menu.SetName( Text.text );
        }
    }
}
