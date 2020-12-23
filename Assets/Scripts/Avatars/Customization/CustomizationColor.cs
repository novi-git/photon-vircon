using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class CustomizationColor : MonoBehaviour
    {
        public Image ColorImage;
        public CustomizationColorPicker ColorPicker;
        public CustomizationArtCategory.CategoryType Category;
        public int ColorIndex;
        public GameObject blockInteractable;

        CustomizationMenu m_Menu;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener( OnClick );

            m_Menu = GetComponentInParent<CustomizationMenu>();
        }

        private IEnumerator Start()
        {
            yield return null;
            OnCustomizationChanged();

            if(ColorImage != null) 
                ColorImage.color = GetCurrentColor();
            
        }

        void OnEnable()
        {
            CustomizationData.Instance.OnCustomizationObjectsChanged += OnCustomizationChanged;
            OnCustomizationChanged();
        }
        private void OnDisable()
        {
            CustomizationData.Instance.OnCustomizationObjectsChanged -= OnCustomizationChanged;
        }

        void OnCustomizationChanged()
        {
            if(ColorImage != null)
                ColorImage.color = GetCurrentColor();
        }

        void OnClick()
        {
            if( ColorPicker != null )
            {
                ColorPicker.Open( GetCurrentColor(), OnColorChanged, OnColorPickerClosed );
                //CustomizationTabs.Instance.gameObject.SetActive( false );
                blockInteractable.SetActive(true);
            }
            else
            {
                if(ColorImage != null)
                    OnColorChanged( ColorImage.color );
            }
        }

        void OnColorPickerClosed()
        {
            CustomizationTabs.Instance.gameObject.SetActive( true );
            blockInteractable.SetActive(false); // Hide blcking the input

            if(ColorImage != null) 
                ColorImage.color = GetCurrentColor();
                
            CustomizationData.Instance.SaveToPlayerPrefs();

            m_Menu?.TryPlayCustomizationAnimation();
        }

        Color GetCurrentColor()
        {
            if( ColorPicker != null )
            {
                return CustomizationData.Instance.GetObjectColor( Category, ColorIndex );
            }
            else
            { 
                return ColorImage.color;
            }
        }

        void OnColorChanged( Color color )
        {
            CustomizationData.Instance.SetObjectColor( Category, ColorIndex, color, false );
        }
    }
}
