using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarXR : MonoBehaviour
{
    public float MoveToLerpSpeed = 5;
    public float RotateTowardsLerpSpeed = 8;
    public float FixedHeadHeight = 1.8f;

    XRSystem m_XrSystem;
    XRSystem.XRSystemData m_TargetData;
    XRSystem.XRSystemData m_DefaultPositionData;

    bool m_HasData;

    void Awake()
    {
        m_XrSystem = GetComponent<XRSystem>();
    }

    private void Start()
    {
        m_XrSystem.StoreLocal( ref m_DefaultPositionData );
    }

    private void Update()
    {
        if( m_HasData == true )
        {
            float heightDifference = FixedHeadHeight - m_TargetData.HeadPosition.y + transform.position.y;
            m_TargetData.HeadPosition.y += heightDifference;
            m_TargetData.LeftHandPosition.y += heightDifference;
            m_TargetData.RightHandPosition.y += heightDifference;

            m_XrSystem.MoveTo( m_TargetData, MoveToLerpSpeed * Time.deltaTime, RotateTowardsLerpSpeed * Time.deltaTime );
        }
        else
        {
            m_XrSystem.MoveToLocal( m_DefaultPositionData, 2 * MoveToLerpSpeed * Time.deltaTime, 2 * RotateTowardsLerpSpeed * Time.deltaTime );
        }
    }

    public void DisableXrSynchronization()
    {
        m_HasData = false;
    }

    public void SetXRSystem( ExitGames.Client.Photon.Hashtable data )
    {
        m_XrSystem.Deserialize( ref m_TargetData, data );

        m_HasData = true;
    }
}
