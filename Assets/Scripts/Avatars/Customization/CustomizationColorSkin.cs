using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class CustomizationColorSkin : MonoBehaviour
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
            ColorImage.color = CustomizationData.Instance.SkinColor;
        }

        void OnClick()
        {
            ColorPicker.Open( CustomizationData.Instance.SkinColor, m_Menu.SetSkinColor, delegate () 
            { 
                CustomizationTabs.Instance.gameObject.SetActive( true );
                ColorImage.color = CustomizationData.Instance.SkinColor;
                m_Menu.TryPlayCustomizationAnimation();
            } );

            CustomizationTabs.Instance.gameObject.SetActive( false );            
        }
    }
}
