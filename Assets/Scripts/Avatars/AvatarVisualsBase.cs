using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public abstract class AvatarVisualsBase : MonoBehaviour
    {
        AvatarCustomization m_Customizer;
        protected virtual void Awake()
        {
            m_Customizer = GetComponentInParent<AvatarCustomization>();
            if( m_Customizer == null )
            {
                DestroyImmediate( this );
                return;
            }

            m_Customizer.CustomizationChangedCallback += ApplyCustomization;
            m_Customizer.SkinColorChangedCallback += ApplySkinColor;
        }

        private void Start()
        {
            m_Customizer?.ApplyCustomization();
        }

        private void OnDestroy()
        {
            if( m_Customizer )
            {
                m_Customizer.CustomizationChangedCallback -= ApplyCustomization;
                m_Customizer.SkinColorChangedCallback -= ApplySkinColor;
            }
        }

        protected virtual void Update()
        {
            UpdateTalking( m_Customizer.IsTalking );
        }

        public abstract void ApplyCustomization( Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> data );

        protected GameObject ReplaceObject( Transform parent, CustomizationArtCategory.CategoryType type, Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> data )
        {
            if( data.ContainsKey( type ) )
            {
                Helpers.DestroyAllChildren( parent );
                return data[ type ].InstantiateAtParent( parent );
            }
            return null;
        }

        public virtual void ApplySkinColor( List<Renderer> renderer )
        {

        }

        public virtual void UpdateTalking( bool isTalking )
        {

        }
    }
}
