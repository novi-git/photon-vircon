using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class Space : MonoBehaviour
    {
        public byte InterestGroup;

        SpaceObject[] m_Objects;

        private void Start()
        {
            m_Objects = GetComponentsInChildren<SpaceObject>();

            SpacesManager.Instance.RegisterSpace( this );   
        }

        private void OnDestroy()
        {
            SpacesManager.Instance.UnregisterSpace( this );
        }

        public void LoadData( Dictionary<string, object> data )
        {
            Debug.Log( "Load space data for " + InterestGroup );

            foreach( var spaceObject in m_Objects )
            {
                spaceObject.OnDataLoaded( data );
            }
        }
    }
}
