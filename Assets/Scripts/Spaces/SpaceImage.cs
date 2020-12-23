using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SpaceImage : SpaceObject
    {
        BoothDisplay[] m_Displays;

        private void Awake()
        {
            m_Displays = GetComponents<BoothDisplay>();
        }

        public override void OnDataLoaded( Dictionary<string, object> data )
        {
            if( data.ContainsKey( "Data_Image" ) )
            {
                foreach( var url in (object[])data[ "Data_Image" ] )
                {
                    foreach( var display in m_Displays )
                    {
                        display.SetImageUrl( url.ToString() );
                    }
                }
            }
        }
    }
}
