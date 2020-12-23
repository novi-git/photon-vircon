using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class CustomizationCategoryButton : MonoBehaviour
    {
        public Transform AttachParent;

        CustomizationObject.Data m_ObjectData;

        public void SetData( CustomizationObject.Data data )
        {
            if( data.Asset != null && data.Asset.GetPrefab() != null )
            {
                GameObject newObject = data.InstantiateAtParent( AttachParent, false );
                newObject.layer = LayerMask.NameToLayer( "UI" );

                for( int i = 0; i < newObject.transform.childCount; ++i )
                {
                    newObject.transform.GetChild( i ).gameObject.layer = LayerMask.NameToLayer( "UI" );
                }                
            }

            m_ObjectData = data;

            var offsets = CustomizationData.Instance.ArtPacks[ m_ObjectData.PackIndex ].Categories[ m_ObjectData.CategoryIndex ].PreviewButtonOffset;
            AttachParent.localPosition = offsets.Offset;
            AttachParent.localScale = offsets.Scale;
            AttachParent.localRotation = Quaternion.Euler( offsets.Rotation );
        }

        public void OnClick()
        {
            CustomizationData.Instance.SetCustomizationObject( m_ObjectData );
        }
    }
}