using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class AvatarCustomization : MonoBehaviour
    {
        public const string AvatarCustomizationData = "ac";
        public bool ApplyLocalCustomizationData;

        public Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> CurrentCustomizationObjects
        {
            get
            {
                if( ApplyLocalCustomizationData )
                {
                    return CustomizationData.Instance.CurrentCustomizationObjects;
                }
                return m_CurrentCustomizationObjects;
            }
        }


        public bool LOD;

        Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> m_CurrentCustomizationObjects = new Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data>();
        public UnityAction<Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data>> CustomizationChangedCallback;


        public Transform BasePrefabParent;
        public bool CanBeCustomized;

        public bool IsTalking { get; set; }

        public UnityAction<List<Renderer>> SkinColorChangedCallback;
        List<Renderer> m_SkinRenderers = new List<Renderer>();

        private void Start()
        {
            var avatar = GetComponent<Avatar>();
            if( CanBeCustomized == false && avatar != null && avatar.CustomProperties != null )
            {
                avatar.CustomProperties.RegisterPlayerPropertiesChangedCallback( OnPlayerPropertiesChanged );
            }

            GameObject prefab = CustomizationData.Instance.GetAvatarBasePrefab();
            if( prefab != null )
            {
                Helpers.DestroyAllChildren( BasePrefabParent );
                Instantiate( prefab, BasePrefabParent );
            }
            else
            {
                Debug.LogError( "Could not find Avatar Base Prefab. Maybe no Artpacks are enabled?" );
            }

            if( ApplyLocalCustomizationData == true )
            {
                CustomizationData.Instance.OnCustomizationObjectsChanged += ApplyCustomization;
                CustomizationData.Instance.ApplyCustomization( this );
            }
            else
            {
                //avatar.CustomProperties.OnPlayerPropertiesChanged( avatar.CustomProperties.Player.CustomProperties );
            }
        }

        public void AddSkinRenderer( Renderer other )
        {
            m_SkinRenderers.Add( other );
        }
        public void RemoveSkinRenderer( Renderer other )
        {
            m_SkinRenderers.Remove( other );
        }

        public void SetCustomizationData( CustomizationArtCategory.CategoryType type, CustomizationObject.Data data )
        {
            if( CurrentCustomizationObjects.ContainsKey( type ) == false )
            {
                CurrentCustomizationObjects.Add( type, data );
            }
            else
            {
                CurrentCustomizationObjects[ type ] = data;
            }
        }

        public void ApplyCustomization()
        {
            CustomizationChangedCallback?.Invoke( CurrentCustomizationObjects );
            SkinColorChangedCallback?.Invoke( m_SkinRenderers );
        }


        void OnPlayerPropertiesChanged( ExitGames.Client.Photon.Hashtable changedProps )
        {
            for( int i = 0; i < (int)CustomizationArtCategory.CategoryType.Count; ++i )
            {
                var type = (CustomizationArtCategory.CategoryType)i;
                var key = GetKeyFromCategoryType( type );

                if( changedProps.ContainsKey( key ) )
                {
                    var data = (CustomizationObject.Data)changedProps[ key ];

                    SetCustomizationData( type, data );
                }
            }

            ApplyCustomization();
        }

        public static string GetKeyFromCategoryType( CustomizationArtCategory.CategoryType type )
        {
            return AvatarCustomizationData + "_" + CustomizationData.Instance.GetArtpackPrefix() + "_" + type.ToString();
        }
    }
}
