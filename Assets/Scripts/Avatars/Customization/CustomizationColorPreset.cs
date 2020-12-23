using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class CustomizationColorPreset : MonoBehaviour
    {
        public Image[] ColorImage;

        public CustomizationArtCategory.CategoryType Category;
        public Color32[] Colors;
        CustomizationMenu m_Menu;


        private void OnValidate()
        {
            UpdateColors();
        }

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener( OnClick );

            UpdateColors();

            m_Menu = GetComponentInParent<CustomizationMenu>();
        }

        void OnClick()
        {
            OnColorChanged();
        }

        void UpdateColors()
        {
            for( int i = 0; i < ColorImage.Length; ++i )
            {
                ColorImage[ i ].color = Colors[ i ];
            }
        }

        void OnColorChanged()
        {
            for( int i = 0; i < Colors.Length; ++i )
            {
                CustomizationData.Instance.SetObjectColor( Category, i, Colors[i], false );
            }
        }
    }
}
