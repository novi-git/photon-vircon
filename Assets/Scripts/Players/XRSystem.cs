using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRSystem : MonoBehaviour
{
    public struct XRSystemData
    {
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;

        public Vector3 LeftHandPosition;
        public Quaternion LeftHandRotation;

        public Vector3 RightHandPosition;
        public Quaternion RightHandRotation;
    }

    public Transform Root;
    public Transform Head;
    public Transform LeftHand;
    public Transform RightHand;

    XRSystemData m_Data = new XRSystemData();

    //Vector3 m_CurrentPosition;
    //Quaternion m_CurrentRotation;

    private void Update()
    {
        //m_CurrentPosition = Root.position;
        //m_CurrentRotation = Root.rotation;

        /*m_Data.HeadPosition = Root.InverseTransformPoint( Head.position );
        m_Data.HeadRotation = Quaternion.Inverse( Root.rotation ) * Head.rotation;

        m_Data.LeftHandPosition = Root.InverseTransformPoint( LeftHand.position );
        m_Data.LeftHandRotation = Quaternion.Inverse( Root.rotation ) * LeftHand.rotation;

        m_Data.RightHandPosition = Root.InverseTransformPoint( RightHand.position );
        m_Data.RightHandRotation = Quaternion.Inverse( Root.rotation ) * RightHand.rotation;*/

        Store( ref m_Data );
    }

    public void Store( ref XRSystemData data )
    {
        data.HeadPosition = Head.position;
        data.HeadRotation = Head.rotation;

        data.LeftHandPosition = LeftHand.position;
        data.LeftHandRotation = LeftHand.rotation;

        data.RightHandPosition = RightHand.position;
        data.RightHandRotation = RightHand.rotation;
    }

    public void StoreLocal( ref XRSystemData data )
    {
        data.HeadPosition = Head.localPosition;
        data.HeadRotation = Head.localRotation;

        data.LeftHandPosition = LeftHand.localPosition;
        data.LeftHandRotation = LeftHand.localRotation;

        data.RightHandPosition = RightHand.localPosition;
        data.RightHandRotation = RightHand.localRotation;
    }

    public void SetTo( XRSystemData other )
    {
        Head.position = other.HeadPosition;
        Head.rotation = other.HeadRotation;

        LeftHand.position = other.LeftHandPosition;
        LeftHand.rotation = other.LeftHandRotation;

        RightHand.position = other.RightHandPosition;
        RightHand.rotation = other.RightHandRotation;
    }

    public void SetToLocal( XRSystemData other )
    {
        Head.localPosition = other.HeadPosition;
        Head.localRotation = other.HeadRotation;

        LeftHand.localPosition = other.LeftHandPosition;
        LeftHand.localRotation = other.LeftHandRotation;

        RightHand.localPosition = other.RightHandPosition;
        RightHand.localRotation = other.RightHandRotation;
    }

    public void MoveTo( XRSystemData data, float moveToSpeed, float rotateTowardsSpeed )
    {
        Head.position = Vector3.Lerp( Head.position, data.HeadPosition, moveToSpeed );
        LeftHand.position = Vector3.Lerp( LeftHand.position, data.LeftHandPosition, moveToSpeed );
        RightHand.position = Vector3.Lerp( RightHand.position, data.RightHandPosition, moveToSpeed );

        Head.rotation = Quaternion.Lerp( Head.rotation, data.HeadRotation, rotateTowardsSpeed );
        LeftHand.rotation = Quaternion.Lerp( LeftHand.rotation, data.LeftHandRotation, rotateTowardsSpeed );
        RightHand.rotation = Quaternion.Lerp( RightHand.rotation, data.RightHandRotation, rotateTowardsSpeed );
    }

    public void MoveToLocal( XRSystemData data, float moveToSpeed, float rotateTowardsSpeed )
    {
        Head.localPosition = Vector3.Lerp( Head.localPosition, data.HeadPosition, moveToSpeed );
        LeftHand.localPosition = Vector3.Lerp( LeftHand.localPosition, data.LeftHandPosition, moveToSpeed );
        RightHand.localPosition = Vector3.Lerp( RightHand.localPosition, data.RightHandPosition, moveToSpeed );

        Head.localRotation = Quaternion.Lerp( Head.localRotation, data.HeadRotation, rotateTowardsSpeed );
        LeftHand.localRotation = Quaternion.Lerp( LeftHand.localRotation, data.LeftHandRotation, rotateTowardsSpeed );
        RightHand.localRotation = Quaternion.Lerp( RightHand.localRotation, data.RightHandRotation, rotateTowardsSpeed );
    }

    public ExitGames.Client.Photon.Hashtable Serialize()
    {
        var data = new ExitGames.Client.Photon.Hashtable();

        data.Add( 0, m_Data.HeadPosition );
        data.Add( 1, m_Data.HeadRotation.eulerAngles );

        data.Add( 2, m_Data.LeftHandPosition );
        data.Add( 3, m_Data.LeftHandRotation.eulerAngles );

        data.Add( 4, m_Data.RightHandPosition );
        data.Add( 5, m_Data.RightHandRotation.eulerAngles );

        return data;
    }

    public void Deserialize( ref XRSystemData xrSystemData, ExitGames.Client.Photon.Hashtable data )
    {
        xrSystemData.HeadPosition = (Vector3)data[ 0 ];
        xrSystemData.HeadRotation = Quaternion.Euler( (Vector3)data[ 1 ] );

        xrSystemData.LeftHandPosition = (Vector3)data[ 2 ];
        xrSystemData.LeftHandRotation = Quaternion.Euler( (Vector3)data[ 3 ] );

        xrSystemData.RightHandPosition = ( Vector3)data[ 4 ];
        xrSystemData.RightHandRotation = Quaternion.Euler( (Vector3)data[ 5 ] );
    }
}
