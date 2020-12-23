using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class CustomizationObject : MonoBehaviour
    {
        [System.Serializable]
        public struct Data
        {
            public byte PackIndex;
            public byte CategoryIndex;
            public byte AssetIndex;

            public Color32[] ColorValues;
            public float[] FloatValues;

            public CustomizationArtCategory.CustomizationAsset Asset;

            public void InitializeReferences()
            {
                if( Asset == null )
                {
                    Asset = CustomizationData.Instance.ArtPacks[ PackIndex ].Categories[ CategoryIndex ].Assets[ AssetIndex ];
                }
                if ( Asset.Category == null )
                {
                    Asset.Category = CustomizationData.Instance.ArtPacks[ PackIndex ].Categories[ CategoryIndex ];
                }

            }

            public GameObject InstantiateAtParent( Transform parent, bool applyCustomValues = true )
            {
                InitializeReferences();

                GameObject newAsset = Instantiate( Asset.GetPrefab() );
                newAsset.transform.SetParent( parent );
                newAsset.transform.localPosition = Vector3.zero;
                newAsset.transform.localRotation = Quaternion.identity;
                newAsset.transform.localScale = Vector3.one;

                CustomizationObject newCustomizationObject = newAsset.GetComponent<CustomizationObject>();
                newCustomizationObject.SetData( this, applyCustomValues );

                return newAsset;
            }
        } // end of Data

        public Data ObjectData { get; private set; }
        public Renderer Renderer;
        public MeshFilter MeshFilter;

        public Color32[] DefaultColors;

        public MaterialPropertyBlock m_MatBlock;

        private void Awake()
        {
            m_MatBlock = new MaterialPropertyBlock();
        }

        public void SetData( Data data, bool applyCustomValues = true )
        {
            ObjectData = data;

            if( ObjectData.Asset.Mesh != null && MeshFilter != null )
            {
                MeshFilter.mesh = ObjectData.Asset.Mesh;
            }
            if ( ObjectData.Asset.Texture != null && Renderer != null )
            {
                Renderer.material.SetTexture( "_MainTex", data.Asset.Texture );
            }

            if( Renderer != null && applyCustomValues == true )
            {
                Color32 defaultColor = data.ColorValues != null && data.ColorValues.Length > 0 ? data.ColorValues[ 0 ] : new Color32( 255, 255, 255, 255 );
                //Debug.Log( ObjectData.ColorValues.GetHashCode().ToString() + " DefaultColor " + gameObject.name + ", " + MeshFilter.mesh.name + " : " + defaultColor.ToString(), gameObject );
                //m_MatBlock.SetColor( "_BaseColor", defaultColor );
                Renderer.material.SetColor( "_BaseColor", defaultColor );

                if( data.ColorValues != null )
                {
                    for( int i = 1; i < data.ColorValues.Length; ++i )
                    {
                        m_MatBlock.SetColor( "_Color" + i, defaultColor );
                        Renderer.material.SetColor( "_Color" + i, data.ColorValues[ i ] );
                    }
                }

                //Renderer.SetPropertyBlock( m_MatBlock );
            }
        }

        public Color32[] InstantiateColors()
        {
            return (Color32[])DefaultColors.Clone();
        }

        public float[] InstantiateFloats()
        {
            return new float[ 0 ];
        }
    }
}