using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class UISoundsButtonConfirm : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var button = GetComponent<Button>();


            if( button != null )
            {
                button.onClick.AddListener( delegate ()
                {
                    UISounds.PlayConfirm();
                } );
            }

            var toggle = GetComponent<Toggle>();

            if( toggle != null )
            {
                toggle.onValueChanged.AddListener( delegate ( bool newValue )
                {
                    if( newValue == true )
                    {
                        UISounds.PlayConfirm();
                    }
                } );
            }
        }
    }
}