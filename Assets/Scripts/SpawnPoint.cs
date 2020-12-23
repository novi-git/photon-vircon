using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{


    public class SpawnPoint : MonoBehaviour
    {
        public const string Default = "Default";

        static Dictionary<string, SpawnPoint> m_SpawnPoints = new Dictionary<string, SpawnPoint>();

        public string Id;

        public static SpawnPoint GetSpawnPositionById( string id )
        {

            if( string.IsNullOrEmpty( id ) == false && m_SpawnPoints.ContainsKey( id ) )
            {
                return m_SpawnPoints[ id ];
            }

            return GetDefaultSpawnPosition();
        }

        public static SpawnPoint GetDefaultSpawnPosition()
        {
            if( m_SpawnPoints.ContainsKey( SpawnPoint.Default ) )
            {
                return m_SpawnPoints[ SpawnPoint.Default ];
            }
            Debug.Log($"Unable To Locate Default Spawn Point: {SpawnPoint.Default}");
            var go = new GameObject( "DefaultSpawnpoint" );
            var spawnpoint = go.AddComponent<SpawnPoint>();
            return spawnpoint;
        }


        private void OnEnable()
        {
            if( string.IsNullOrEmpty( Id ) )
            {
                Id = SpawnPoint.Default;
            }
            if( m_SpawnPoints.ContainsKey( Id ) )
            {
                return;
            }
            m_SpawnPoints.Add( Id, this );
        }
        private void OnDisable()
        {
            if( m_SpawnPoints.ContainsKey( Id ) )
            {
                m_SpawnPoints.Remove( Id );
            }
        }
    }
}
