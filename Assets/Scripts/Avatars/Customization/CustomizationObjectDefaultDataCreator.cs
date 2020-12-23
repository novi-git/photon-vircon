using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    [RequireComponent( typeof( CustomizationObject ) )]
    public class CustomizationObjectDefaultDataCreator : MonoBehaviour
    {
        public CustomizationArtCategory.CategoryType Type;

        void Start()
        {
            if ( CustomizationData.Instance.CurrentCustomizationObjects.ContainsKey(Type) )
            {
                return;
            }

            var data = CustomizationData.Instance.GetDefaultDataForAsset( Type );
            GetComponent<CustomizationObject>().SetData( data );
            var customizer = GetComponentInParent<AvatarCustomization>();
            customizer.SetCustomizationData( Type, data );

            if( customizer.CanBeCustomized )
            {
                CustomizationData.Instance.SetCustomizationObject( data, false, false );
            }
        }
    }
}
