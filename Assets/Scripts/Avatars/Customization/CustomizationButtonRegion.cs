using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.ProBuilder;

namespace SpaceHub.Conference
{
    public class CustomizationButtonRegion : MonoBehaviour
    {
        public TextMeshProUGUI Text;
        public Color NormalColor;
        public Color SelectedColor;

        Button m_Button;
        string m_RegionString;

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener( OnClick );
        }

        private void Start()
        {
            m_RegionString = Text.text.ToLower();
        }

        private void FixedUpdate()
        {
            bool isSelected = PlayerPrefs.GetString( "PhotonRegion", "eu" ) == m_RegionString;

            var colors = m_Button.colors;
            colors.normalColor = isSelected ? SelectedColor : NormalColor;
            colors.selectedColor = colors.normalColor;
            m_Button.colors = colors;
        }

        void OnClick()
        {
            PlayerPrefs.SetString( "PhotonRegion", m_RegionString );
        }
    }
}
