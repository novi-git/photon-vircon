using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class AvatarVisualsDefault : AvatarVisualsBase
    {
        public const string ColorParameterName = "_BaseColor";

        public Material BaseMaterial;
        public Renderer[] SkinRenderers;
        public Transform[] HandParents;

        public Renderer[] ShirtRenderer;
        public Transform HairParent;
        public Transform BodyParent; 
        public Transform LowerBodyParent; 

        public Transform AttachmentHeadParent;
        public Transform AttachmentEyeBrowParent;
        public Transform AttachmentFacialHairParent;

        public Color SkinColor = Color.blue;
        public Color ShirtColor = Color.green;

        public static Dictionary<string, Material> s_Materials = new Dictionary<string, Material>();

        Material m_SkinMaterial;

        public float TalkSwapInterval = 0.2f;
        public GameObject ClosedMouth;
        public GameObject[] TalkingMouths;
        float m_NextMouthSwap;
        int m_CurrentMouthIndex = 0;

        public override void ApplyCustomization( Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> data )
        {
            Material material;

            ReplaceObject( BodyParent, CustomizationArtCategory.CategoryType.UpperBody, data );
            ReplaceObject( HairParent, CustomizationArtCategory.CategoryType.Hair, data );
            ReplaceObject( LowerBodyParent, CustomizationArtCategory.CategoryType.LowerBody, data ); 
            
            ReplaceObject( AttachmentHeadParent, CustomizationArtCategory.CategoryType.AttachmentsHead, data ); 
            ReplaceObject( AttachmentEyeBrowParent, CustomizationArtCategory.CategoryType.AttachmentsEyeBrow, data );
            ReplaceObject( AttachmentFacialHairParent, CustomizationArtCategory.CategoryType.AttachmentsFacialHair, data );

            // Change color of bodytype


            if( data.ContainsKey( CustomizationArtCategory.CategoryType.Face ) )
            {
                material = GetMaterial( data[ CustomizationArtCategory.CategoryType.Face ].ColorValues[ 0 ] );
                SetMaterialForRenderers( SkinRenderers, material );
                foreach( var handparent in HandParents )
                {
                    SetMaterialForRenderers( handparent.GetComponentsInChildren<SkinnedMeshRenderer>(), material );
                }
                m_SkinMaterial = material;
            }

        }

        public override void UpdateTalking( bool isTalking )
        {
            if ( Time.realtimeSinceStartup < m_NextMouthSwap )
            {
                return;
            }

            m_NextMouthSwap = Time.realtimeSinceStartup + TalkSwapInterval;

            if( isTalking )
            {
                HideAllTalking();
                int newMouthIndex = 0;
                do
                {
                    newMouthIndex = Random.Range( 0, TalkingMouths.Length );
                } while( newMouthIndex == m_CurrentMouthIndex );
                m_CurrentMouthIndex = newMouthIndex;

                TalkingMouths[ m_CurrentMouthIndex ].SetActive( true );
            }
            else
            {
                HideAllTalking();
            }

            if( ClosedMouth != null && ClosedMouth.activeSelf == isTalking )
            {
                ClosedMouth.SetActive( !isTalking );
            }
        }

        void HideAllTalking()
        {
            foreach( var mouth in TalkingMouths )
            {
                if( mouth != null && mouth.activeSelf )
                {
                    mouth.SetActive( false );
                }
            }
        }
        Material GetMaterial( Color color )
        {
            string code = ColorUtility.ToHtmlStringRGB( color );
            if( s_Materials.ContainsKey( code ) == false )
            {
                var mat = new Material( BaseMaterial );
                mat.SetColor( ColorParameterName, color );
                s_Materials.Add( code, mat );
            }
            return s_Materials[ code ];

        }

        public override void ApplySkinColor( List<Renderer> renderer )
        {
            SetMaterialForRenderers( renderer.ToArray(), m_SkinMaterial );
        }

        void SetMaterialForRenderers( Renderer[] renderers, Material material )
        {
            foreach( var rend in renderers )
            {
                rend.material = material;
            }
        }
    }
}
