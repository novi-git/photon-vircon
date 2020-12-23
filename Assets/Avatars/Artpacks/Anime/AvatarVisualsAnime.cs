using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class AvatarVisualsAnime : AvatarVisualsBase
    {
        public Material BaseSkinMaterial;
        public Renderer[] SkinRenderers;
        Material m_Skin;
        public Transform HeadParent;
        public Transform HairParent;
        public Transform EyesParent;
        public Transform UpperBodyParent;
        public Transform LowerBodyParent;
        public Transform BodyAttachmentParent;
        public Transform HeadAttachmentParent;

        protected override void Awake()
        {
            base.Awake();
            m_Skin = new Material( BaseSkinMaterial );
        }

        public override void ApplyCustomization( Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> data )
        {
            if( data.ContainsKey( CustomizationArtCategory.CategoryType.Face ) )
            {
                foreach( var rend in SkinRenderers )
                {
                    rend.sharedMaterial = m_Skin;
                }
                var go = ReplaceObject( HeadParent, CustomizationArtCategory.CategoryType.Face, data );
                if( go != null )
                {
                    var rend = go.GetComponentInChildren<Renderer>();
                    if ( rend != null )
                    {
                        rend.sharedMaterial = m_Skin;
                    }
                }
                m_Skin.SetColor( "_Color1", data[ CustomizationArtCategory.CategoryType.Face ].ColorValues[ 0 ] );
            }

            ReplaceObject( EyesParent, CustomizationArtCategory.CategoryType.Eyes, data );
            

            ReplaceObject( HairParent, CustomizationArtCategory.CategoryType.Hair, data );
            ReplaceObject( LowerBodyParent, CustomizationArtCategory.CategoryType.LowerBody, data );
            ReplaceObject( UpperBodyParent, CustomizationArtCategory.CategoryType.UpperBody, data );

            ReplaceObject( HeadAttachmentParent, CustomizationArtCategory.CategoryType.AttachmentsHead, data );
            ReplaceObject( BodyAttachmentParent, CustomizationArtCategory.CategoryType.AttachmentsBody, data );
        }

       

        public override void ApplySkinColor( List<Renderer> renderer )
        {
            foreach( var rend in renderer )
            {
                rend.sharedMaterial = m_Skin;
            }
        }

    }
}
