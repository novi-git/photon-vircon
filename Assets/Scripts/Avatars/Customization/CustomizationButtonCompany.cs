using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceHub.Conference
{
    public class CustomizationButtonCompany : MonoBehaviour
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
            menu.SetCompany( Text.text );
        }
    }
}
