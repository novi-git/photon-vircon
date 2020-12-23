using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfineXRPosition : MonoBehaviour
{
    public Transform TargetTransform;
    public Transform OffsetTransform;
    public Transform HeadTransform;
    public float Radius = 0.5f;

    public float Interval = 1f;

    float m_NextCheck = 0f;

    void Update()
    {
        if ( Time.realtimeSinceStartup < m_NextCheck )
        {
            return;
        }

        Vector3 dir = TargetTransform.position - HeadTransform.position;
        dir.y = 0f;

        if ( dir.magnitude > Radius )
        {
            OffsetTransform.position += dir;
        }
    }

    private void OnDrawGizmos()
    {
        if ( TargetTransform == null)
        {
            return;
        }

        Gizmos.DrawWireSphere( TargetTransform.position, Radius );
    }
}
