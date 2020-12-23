using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class CustomizationColorButtons : MonoBehaviour
    {
        public GameObject ButtonPrefab;
        public CustomizationArtCategory.CategoryType Type;
        public CustomizationColorPicker ColorPicker;
        public int ButtonsCount;

        public List<CustomizationColor> m_Buttons = new List<CustomizationColor>();

        void Start()
        {
            CreateButtons();
        }


        private void OnEnable()
        {
            if( CustomizationData.Instance != null )
            {
                CustomizationData.Instance.OnCustomizationObjectsChanged += OnObjectChanged;
            }
            OnObjectChanged();
        }

        private void OnDisable()
        {
            if( CustomizationData.Instance != null )
            {
                CustomizationData.Instance.OnCustomizationObjectsChanged -= OnObjectChanged;
            }
        }

        void CreateButtons()
        {
            for( int i = 0; i < ButtonsCount; i++ )
            {
                GameObject newButton = Instantiate( ButtonPrefab, transform );
                RectTransform rectTransform = newButton.GetComponent<RectTransform>();

                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                var script = newButton.GetComponent<CustomizationColor>();
                script.ColorIndex = i;
                script.ColorPicker = ColorPicker;
                script.Category = Type;

                m_Buttons.Add( script );
            }
            OnObjectChanged();
        }

        public void OnObjectChanged()
        {
            if( CustomizationData.Instance.CurrentCustomizationObjects.ContainsKey( Type ) )
            {
                CustomizationObject.Data data = CustomizationData.Instance.CurrentCustomizationObjects[ Type ];
                for( int i = 0; i < m_Buttons.Count; ++i )
                {
                    m_Buttons[ i ].gameObject.SetActive( i < data.ColorValues.Length );
                }
            }
            else
            {
                for( int i = 0; i < m_Buttons.Count; ++i )
                {
                    m_Buttons[ i ].gameObject.SetActive( false );
                }
            }

        }
    }
}