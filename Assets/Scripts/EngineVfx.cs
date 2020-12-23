using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineVfx : MonoBehaviour
{
    public Vector3 RotationSpeed;
    public float ScaleSpeed;
    public float ScaleIntensity;

    Vector3 m_BaseScale;
    Vector3 m_Scale;
    private void Awake()
    {
        m_BaseScale = transform.localScale;
    }
    void Update()
    {
        transform.Rotate( RotationSpeed, Space.Self );

        m_Scale = m_BaseScale;
        m_Scale.y += Mathf.Sin( Time.realtimeSinceStartup * ScaleSpeed ) * ScaleIntensity;
        transform.localScale = m_Scale;
    }
}
