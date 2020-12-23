using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageAttendeePlacer : MonoBehaviour
{
    public float AngleVariance;
    public float HeightVariance;

    public float IgnoreDistance;
    public GameObject HighPrefab;
    public float HighDistance;
    public GameObject MidPrefab;
    public float MidDistance;
    public GameObject LowPrefab;

    public Transform UserCenterPoint;

    [System.Serializable]
    public struct RowData
    {
        public int seatCount;
        public float radius;
        public float height;
    }

    public RowData[] Rows;


    public void Place()
    {
        int childcount = transform.childCount;
        for( int i = 0; i < childcount; ++i )
        {
            DestroyImmediate( transform.GetChild( 0 ).gameObject );
        }

        SortedList<float, GameObject> gos = new SortedList<float, GameObject>();

        foreach( var data in Rows )
        {
            float deltaAngle = 360f / data.seatCount;
            for( int i = 0; i < data.seatCount; ++i )
            {
                float angle = (i * deltaAngle + Random.Range( -AngleVariance, AngleVariance ) ) * Mathf.Deg2Rad;
                Vector3 position = new Vector3( Mathf.Cos( angle ), 0f, Mathf.Sin( angle ) );
                Vector3 forward = -position;
                position *= data.radius;
                position.y = data.height + Random.Range(-HeightVariance, HeightVariance );

                position = transform.TransformPoint( position );

                float distance = Vector3.Distance( position, UserCenterPoint.position );
                while( gos.ContainsKey(distance) )
                {
                    distance += 0.001f;
                }

                if( distance < IgnoreDistance )
                {

                }
                else if( distance < HighDistance )
                {
                    gos.Add( distance, Instantiate( HighPrefab, position, Quaternion.LookRotation( forward ), transform ) );
                }
                else if( distance < MidDistance )
                {
                    gos.Add( distance, Instantiate( MidPrefab, position, Quaternion.LookRotation( forward ), transform ) );
                }
                else
                {
                    gos.Add( distance, Instantiate( LowPrefab, position, Quaternion.LookRotation( forward ), transform ) );
                }

            }
        }

        for(int i = 0; i< gos.Count; ++i)
        {
            gos.Values[ i ].transform.SetSiblingIndex( i );
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        foreach( var ring in Rows )
        {
            int interval = 10;
            for( int i = 0; i < 360; i += interval )
            {
                var pos1 = new Vector3( Mathf.Cos( i * Mathf.Deg2Rad ), 0f, Mathf.Sin( i * Mathf.Deg2Rad ) ) * ring.radius;
                var pos2 = new Vector3( Mathf.Cos( ( i + interval ) * Mathf.Deg2Rad ), 0f, Mathf.Sin( ( i + interval ) * Mathf.Deg2Rad ) ) * ring.radius;
                pos1.y = ring.height;
                pos2.y = ring.height;
                Gizmos.DrawLine( pos1, pos2 );
            }
        }
    }
}
