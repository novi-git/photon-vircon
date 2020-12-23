using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class CustomizationTabs : MonoBehaviour
    {
        public static CustomizationTabs Instance;

        public RectTransform TabButtonsParent;
        public RectTransform TabsParent;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CustomizationTabToggle[] toggles = TabButtonsParent.GetComponentsInChildren<CustomizationTabToggle>();

            foreach( var toggle in toggles )
            {
                if( toggle.GetComponent<Toggle>().isOn == true )
                {
                    ShowTab( toggle.Tab );
                }
            }
        }

        public void ShowTab( GameObject tab )
        {
            for( int i = 0; i < TabsParent.childCount; ++i )
            {
                TabsParent.GetChild( i ).gameObject.SetActive( TabsParent.GetChild( i ).gameObject == tab );
            }

        }
    }
}