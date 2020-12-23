using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class UISoundsButton : MonoBehaviour
    {
        public string Type = "Confirm";

        void Start()
        {
            var button = GetComponent<Button>();


            if( button != null )
            {
                button.onClick.AddListener( delegate ()
                {
                    UISounds.Play( Type );
                } );
            }

            var toggle = GetComponent<Toggle>();

            if( toggle != null )
            {
                toggle.onValueChanged.AddListener( delegate ( bool newValue )
                {
                    if( newValue == true )
                    {
                        UISounds.Play( Type );
                    }
                } );
            }
        }
    }
}