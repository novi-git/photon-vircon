using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class CustomizationColorShirt : MonoBehaviour
    {
        public Image ColorImage;
        public CustomizationColorPicker ColorPicker;

        CustomizationMenu m_Menu;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener( OnClick );

            m_Menu = GetComponentInParent<CustomizationMenu>();

        }

        private void Start()
        {
            ColorImage.color = CustomizationData.Instance.ShirtColor;
        }

        void OnClick()
        {
            ColorPicker.Open( CustomizationData.Instance.ShirtColor, m_Menu.SetShirtColor, delegate ()
            {
                CustomizationTabs.Instance.gameObject.SetActive( true );
                ColorImage.color = CustomizationData.Instance.ShirtColor;
                m_Menu.TryPlayCustomizationAnimation();
            } );

            CustomizationTabs.Instance.gameObject.SetActive( false );
        }
    }
}
