using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SpaceClickableUrl : SpaceObject
    {
        string m_Url;

        public void OnClick()
        {
            MessagePopup.ShowConfirm( "Do you want to go to: " + m_Url, "Yes", "No", DoOpenUrl, delegate () { } );
        }

        void DoOpenUrl()
        {
            Helpers.OpenURL( m_Url );
        }

        public override void OnDataLoaded( Dictionary<string, object> data )
        {
            if( data.ContainsKey( "Data_Url" ) )
            {
                m_Url = data[ "Data_Url" ].ToString();
            }
        }
    }
}