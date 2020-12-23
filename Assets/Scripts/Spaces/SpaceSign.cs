using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SpaceSign : SpaceObject
    {
        TextMeshPro m_Text;

        private void Awake()
        {
            m_Text = GetComponentInChildren<TextMeshPro>();
        }

        public override void OnDataLoaded( Dictionary<string, object> data )
        {
            if( data.ContainsKey( "Data_Name" ) )
            {
                m_Text.text = data[ "Data_Name" ].ToString();
            }
        }
    }
}