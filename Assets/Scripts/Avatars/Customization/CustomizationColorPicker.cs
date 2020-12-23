using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class CustomizationColorPicker : MonoBehaviour
    {
        public TMP_InputField ColorInputField;
        public TextMeshProUGUI ColorInputText;

        ColorPickerHueRing m_HueRing;
        ColorPickerSquare m_Square;

        Color m_StartColor;
        UnityAction<Color> m_SetColorCallback;
        UnityAction m_OnCloseCallback;

        private void Awake()
        {
            m_HueRing = GetComponentInChildren<ColorPickerHueRing>();
            m_Square = GetComponentInChildren<ColorPickerSquare>();

            m_HueRing.OnColorChanged += OnColorChanged;
            m_Square.OnColorChanged += OnColorChanged;
        }

        public void Open( Color currentColor, UnityAction<Color> setColorCallback, UnityAction onCloseCallback )
        {
            m_HueRing = GetComponentInChildren<ColorPickerHueRing>();
            m_Square = GetComponentInChildren<ColorPickerSquare>();

            m_StartColor = currentColor;
            m_SetColorCallback = setColorCallback;
            m_OnCloseCallback = onCloseCallback;

            SetColor( currentColor );

            gameObject.SetActive( true );
        }

        public void SetColor( Color color )
        {
            Color.RGBToHSV( color, out float h, out float s, out float v );

            m_HueRing.Set( h, false );
            m_Square.SetHue( h );
            m_Square.Set( s, v, true );
        }

        public void OnColorChanged()
        {
            Color newColor = Color.HSVToRGB( m_HueRing.Hue, m_Square.Saturation, m_Square.Value );

            ColorInputField.GetComponent<Image>().color = newColor;
            ColorInputField.text = "#" + ColorUtility.ToHtmlStringRGB( newColor );
            ColorInputText.color = m_Square.Value > 0.4f ? Color.black : Color.white;

            m_SetColorCallback?.Invoke( newColor );
        }

        public void OnTextfieldChanged()
        {
            if( ColorUtility.TryParseHtmlString( ColorInputField.text, out Color newColor ) )
            {
                SetColor( newColor );
            }
        }

        public void OnConfirmButtonClicked()
        {
            gameObject.SetActive( false );
            m_OnCloseCallback?.Invoke();
        }

        public void OnCancelButtonClicked()
        {
            gameObject.SetActive( false );
            m_SetColorCallback.Invoke( m_StartColor );
            m_OnCloseCallback?.Invoke();
        }
    }
}