using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarNeck : MonoBehaviour
{
    public Transform Target;
    public float DefaultDistance = 0.102f;

    Vector3 m_Scale = new Vector3( 1, 1, 1 );
    private void LateUpdate()
    {
        transform.LookAt( Target, -transform.parent.forward );

        float distance = Vector3.Distance( transform.position, Target.position );
        m_Scale.z = distance / DefaultDistance;
        transform.localScale = m_Scale;
    }
}
