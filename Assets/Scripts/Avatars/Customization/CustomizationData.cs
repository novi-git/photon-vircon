using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.Events;
using ExitGames.Client.Photon;

namespace SpaceHub.Conference
{
    public class CustomizationData : MonoBehaviour
    {
        public static CustomizationData Instance;

        public CustomizationArtPack[] ArtPacks;
        public string ArtPackOverride;
        public Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data> CurrentCustomizationObjects = new Dictionary<CustomizationArtCategory.CategoryType, CustomizationObject.Data>();

        List<CustomizationArtPack.EmoteData> m_EmoteData = null;
        public GameObject[] Hairstyles;
        public GameObject[] HairstylesLOD;

        public Color ShirtColor;
        public Color SkinColor;
        public Color HairColor;
        public byte HairstyleIndex;

        public string Nickname;
        public string CompanyName;

        public UnityAction OnCustomizationObjectsChanged;

        GameObject m_AvatarBasePrefab;

        string m_ArtpackPrefix;

        private void Awake()
        {
            if( Instance != null )
            {
                DestroyImmediate( gameObject );
                return;
            }

            Instance = this;
            DontDestroyOnLoad( gameObject );
        }

        private void Start()
        {
            LoadDefaultStart();
            LoadFromPlayerPrefs();

#if UNITY_EDITOR
            if( !string.IsNullOrEmpty( ArtPackOverride ) )
            {
                LoadArtPack( ArtPackOverride );
            }
#endif
        }

        public void LoadArtPack( string artPackPrefix )
        {
#if UNITY_EDITOR
            if( !string.IsNullOrEmpty( ArtPackOverride ) )
            {
                artPackPrefix = ArtPackOverride;
            }
#endif

            Debug.Log( "Load art pack: " + artPackPrefix );
            bool doesPrefixExist = false;

            foreach( var artPack in ArtPacks )
            {
                if( artPack.Prefix == artPackPrefix )
                {
                    doesPrefixExist = true;
                }
            }

            if( doesPrefixExist == false )
            {
                Debug.LogWarning( "Art pack prefix '" + artPackPrefix + "' does not exist. Fallback to 'default'" );
                artPackPrefix = "default";
            }

            foreach( var artPack in ArtPacks )
            {
                m_ArtpackPrefix = artPackPrefix;
                artPack.IsEnabled = string.IsNullOrEmpty( artPack.Prefix ) || artPack.Prefix == artPackPrefix;
            }
        }

        public Color GetObjectColor( CustomizationArtCategory.CategoryType category, int index = 0 )
        {
            if( CurrentCustomizationObjects.ContainsKey( category ) == false ||
                CurrentCustomizationObjects[ category ].ColorValues.Length <= index )
            {
                return Color.white;
            }

            return CurrentCustomizationObjects[ category ].ColorValues[ index ];
        }

        public float GetObjectFloat( CustomizationArtCategory.CategoryType category, int index = 0 )
        {
            if( CurrentCustomizationObjects.ContainsKey( category ) == false ||
                CurrentCustomizationObjects[ category ].FloatValues.Length >= index )
            {
                return 0f;
            }

            return CurrentCustomizationObjects[ category ].FloatValues[ index ];
        }

        public void SetObjectColor( CustomizationArtCategory.CategoryType category, int index, Color value, bool saveToPlayerPrefs = true )
        {
            if( CurrentCustomizationObjects.ContainsKey( category ) )
            {
                if( CurrentCustomizationObjects[ category ].ColorValues.Length > index )
                {
                    CurrentCustomizationObjects[ category ].ColorValues[ index ] = value;
                }
            }

            OnCustomizationObjectsChanged?.Invoke();

            if( saveToPlayerPrefs == true )
            {
                SaveToPlayerPrefs();
            }
        }

        public void SetObjectFloat( CustomizationArtCategory.CategoryType category, int index, float value, bool saveToPlayerPrefs = true )
        {
            if( CurrentCustomizationObjects.ContainsKey( category ) )
            {
                CurrentCustomizationObjects[ category ].FloatValues[ index ] = value;
            }

            OnCustomizationObjectsChanged?.Invoke();

            if( saveToPlayerPrefs == true )
            {
                SaveToPlayerPrefs();
            }
        }

        public void SetCustomizationObject( CustomizationObject.Data customizationObject, bool keepCustomData = true, bool saveToPlayerPrefs = true )
        {
            customizationObject.InitializeReferences();

            if( customizationObject.Asset == null || customizationObject.Asset.Category == null )
            {
                Debug.LogWarning( "Failed to set Customization Object: Pack:" + customizationObject.PackIndex + ", Category:" + customizationObject.CategoryIndex + " ,AssetIndex:" + customizationObject.AssetIndex );
                return;
            }

            if( !string.IsNullOrEmpty( ArtPacks[ customizationObject.PackIndex ].Prefix ) && ArtPacks[ customizationObject.PackIndex ].Prefix != GetArtpackPrefix() )
            {
                Debug.LogWarning( "Loaded customization object does not fit the currently loaded art pack prefix. Skipping." );
                return;
            }

            var category = customizationObject.Asset.Category.Type;

            if( CurrentCustomizationObjects.ContainsKey( category ) == false )
            {
                CurrentCustomizationObjects.Add( category, customizationObject );
            }
            else
            {
                var newObject = customizationObject;

                if( keepCustomData == true )
                {
                    for( int i = 0; i < Mathf.Min( newObject.ColorValues.Length, CurrentCustomizationObjects[ category ].ColorValues.Length ); ++i )
                    {
                        newObject.ColorValues[ i ] = CurrentCustomizationObjects[ category ].ColorValues[ i ];
                    }
                }

                CurrentCustomizationObjects[ category ] = newObject;
            }

            OnCustomizationObjectsChanged?.Invoke();

            if( saveToPlayerPrefs == true )
            {
                SaveToPlayerPrefs();
            }
        }

        public List<CustomizationArtPack.EmoteData> GetAllEmotes()
        {
            if( m_EmoteData != null )
            {
                return m_EmoteData;
            }

            m_EmoteData = new List<CustomizationArtPack.EmoteData>();

            foreach( var pack in ArtPacks )
            {
                if( pack == null || pack.IsEnabled == false )
                {
                    continue;
                }
                m_EmoteData.AddRange( pack.EmoteAnimations );
            }
            return m_EmoteData;
        }

        public List<AnimationClip> GetCustomizationWelcomeAnimations()
        {
            var result = new List<AnimationClip>();
            foreach( var pack in ArtPacks )
            {
                if( pack == null || pack.IsEnabled == false )
                {
                    continue;
                }
                result.AddRange( pack.CustomizationWelcomeAnimations );
            }
            return result;
        }

        public List<AnimationClip> GetCustomizationAnimations()
        {
            var result = new List<AnimationClip>();
            foreach( var pack in ArtPacks )
            {
                if( pack == null || pack.IsEnabled == false )
                {
                    continue;
                }
                result.AddRange( pack.CustomizationAnimations );
            }
            return result;
        }

        public string GetArtpackPrefix()
        {
            if( string.IsNullOrEmpty( m_ArtpackPrefix ) == false )
            {
                return m_ArtpackPrefix;
            }

            for( byte packIndex = 0; packIndex < ArtPacks.Length; ++packIndex )
            {
                var pack = ArtPacks[ packIndex ];

                if( pack == null || pack.IsEnabled == false )
                {
                    continue;
                }
                if( string.IsNullOrEmpty( pack.Prefix ) )
                {
                    continue;
                }

                m_ArtpackPrefix = pack.Prefix;
                return m_ArtpackPrefix;
            }

            return "default";

        }

        public GameObject GetAvatarBasePrefab()
        {
            if( m_AvatarBasePrefab != null )
            {
                return m_AvatarBasePrefab;
            }

            for( byte packIndex = 0; packIndex < ArtPacks.Length; ++packIndex )
            {
                var pack = ArtPacks[ packIndex ];

                if( pack == null || pack.IsEnabled == false )
                {
                    continue;
                }

                if( pack.BasePrefab != null )
                {
                    m_AvatarBasePrefab = pack.BasePrefab;
                    return m_AvatarBasePrefab;
                }
            }
            return null;
        }

        public CustomizationObject.Data GetDefaultDataForAsset( CustomizationArtCategory.CategoryType categoryType )
        {
            var temp = GetAllAssetsOfCategoryType( categoryType );
            if( temp == null || temp.Count <= 0 )
            {
                Debug.LogError( "Could not find Any (default) asset for Type " + categoryType.ToString() );
                return new CustomizationObject.Data();
            }
            return temp[ 0 ];
        }

        public List<CustomizationObject.Data> GetAllAssetsOfCategoryType( CustomizationArtCategory.CategoryType categoryType )
        {
            List<CustomizationObject.Data> outputList = new List<CustomizationObject.Data>();

            for( byte packIndex = 0; packIndex < ArtPacks.Length; ++packIndex )
            {
                var pack = ArtPacks[ packIndex ];

                if( pack == null || pack.IsEnabled == false )
                {
                    continue;
                }

                for( byte categoryIndex = 0; categoryIndex < pack.Categories.Count; ++categoryIndex )
                {
                    var category = pack.Categories[ categoryIndex ];

                    if( category.Type == categoryType )
                    {
                        for( byte assetIndex = 0; assetIndex < category.Assets.Count; ++assetIndex )
                        {
                            var asset = category.Assets[ assetIndex ];
                            asset.Category = category;

                            outputList.Add( CreateCustomizationObjectData( packIndex, categoryIndex, assetIndex, asset ) );
                        }
                    }
                }
            }

            return outputList;
        }

        public CustomizationObject.Data CreateCustomizationObjectData( byte packIndex, byte categoryIndex, byte assetIndex, CustomizationArtCategory.CustomizationAsset asset )
        {
            var colors = asset.GetPrefab()?.GetComponent<CustomizationObject>()?.InstantiateColors();
            var floats = asset.GetPrefab()?.GetComponent<CustomizationObject>()?.InstantiateFloats();

            if( colors == null )
            {
                colors = new Color32[ 0 ];
            }
            if( floats == null )
            {
                floats = new float[ 0 ];
            }

            return new CustomizationObject.Data()
            {
                PackIndex = packIndex,
                CategoryIndex = categoryIndex,
                AssetIndex = assetIndex,
                Asset = asset,
                ColorValues = colors,
                FloatValues = floats,
            };
        }

        public void LoadFromPlayerPrefs()
        {
            //NOTE: Avatar system is in progress of being refactor, so at the moment this is a weird-mashup of the old and the new system

            Nickname = PlayerPrefs.GetString( ConferenceCustomProperties.NickNamePropertyName, Nickname );
            CompanyName = PlayerPrefs.GetString( ConferenceCustomProperties.CompanyNamePropertyName, CompanyName );
            //ColorUtility.TryParseHtmlString( "#" + PlayerPrefs.GetString( AvatarCustomization.SkinColorPropertyName, ColorUtility.ToHtmlStringRGB( SkinColor ) ), out SkinColor );
            //ColorUtility.TryParseHtmlString( "#" + PlayerPrefs.GetString( AvatarCustomization.HairColorPropertyName, ColorUtility.ToHtmlStringRGB( HairColor ) ), out HairColor );
            //ColorUtility.TryParseHtmlString( "#" + PlayerPrefs.GetString( AvatarCustomization.ShirtColorPropertyName, ColorUtility.ToHtmlStringRGB( ShirtColor ) ), out ShirtColor );
            //HairstyleIndex = (byte)PlayerPrefs.GetInt( AvatarCustomization.HairStylePropertyName, HairstyleIndex );

            foreach( CustomizationArtCategory.CategoryType type in System.Enum.GetValues( typeof( CustomizationArtCategory.CategoryType ) ) )
            {
                LoadCategoryTypeFromPlayerPrefs( type );
            }


            OnCustomizationObjectsChanged?.Invoke();
        }


        public void SaveToPlayerPrefs()
        {
            //NOTE: Avatar system is in progress of being refactor, so at the moment this is a weird-mashup of the old and the new system

            PlayerPrefs.SetString( ConferenceCustomProperties.NickNamePropertyName, Nickname );
            PlayerPrefs.SetString( ConferenceCustomProperties.CompanyNamePropertyName, CompanyName );
            //PlayerPrefs.SetString( AvatarCustomization.SkinColorPropertyName, ColorUtility.ToHtmlStringRGB( SkinColor ) );
            //PlayerPrefs.SetString( AvatarCustomization.HairColorPropertyName, ColorUtility.ToHtmlStringRGB( HairColor ) );
            //PlayerPrefs.SetString( AvatarCustomization.ShirtColorPropertyName, ColorUtility.ToHtmlStringRGB( ShirtColor ) );
            //PlayerPrefs.SetInt( AvatarCustomization.HairStylePropertyName, HairstyleIndex );

            foreach( CustomizationArtCategory.CategoryType type in System.Enum.GetValues( typeof( CustomizationArtCategory.CategoryType ) ) )
            {
                SaveCategoryTypeToPlayerPrefs( type );
            }

            PlayerPrefs.Save();
        }
        void LoadDefaultStart(){

            /* 
            Face : AAEAAQD/w6A=
            Hair : AAABAQABAQE=
            UpperBody : AAMDAQABAQE=
            LowerBody : AAQAAQAFAWw=
            AttachmentsHead : AAIBAQBZOhw=
            AttachmentsEyeBrow : AAUEAQBZOhw=
            */
            DeserializeProperty( CustomizationArtCategory.CategoryType.Face, "AAEAAQD/w6A=");
            DeserializeProperty( CustomizationArtCategory.CategoryType.Hair, "AAABAQABAQE=");
            DeserializeProperty(  CustomizationArtCategory.CategoryType.UpperBody, "AAMDAQABAQE=");
            DeserializeProperty(  CustomizationArtCategory.CategoryType.LowerBody, "AAQAAQAFAWw=");
            DeserializeProperty(  CustomizationArtCategory.CategoryType.AttachmentsHead, "AAIBAQBZOhw="); 
            DeserializeProperty(  CustomizationArtCategory.CategoryType.AttachmentsEyeBrow, "AAUEAQBZOhw=");

          /*   foreach( CustomizationArtCategory.CategoryType type in System.Enum.GetValues( typeof( CustomizationArtCategory.CategoryType ) ) )
            { 
                Debug.Log(type + " : " + PlayerPrefs.GetString( AvatarCustomization.GetKeyFromCategoryType( type )));
            }    */
        }
        void LoadCategoryTypeFromPlayerPrefs( CustomizationArtCategory.CategoryType type )
        {

            DeserializeProperty( type, PlayerPrefs.GetString( AvatarCustomization.GetKeyFromCategoryType( type ) ) );
        }

        void SaveCategoryTypeToPlayerPrefs( CustomizationArtCategory.CategoryType type )
        {
            PlayerPrefs.SetString( AvatarCustomization.GetKeyFromCategoryType( type ), SerializeProperty( type ) );
        }

        void DeserializeProperty( CustomizationArtCategory.CategoryType type, string serializedValue )
        {
            if( serializedValue == "" )
            {
                return;
            }

            var bytes = System.Convert.FromBase64String( serializedValue );

            CustomizationObject.Data data = (CustomizationObject.Data)CustomTypes.DeserializeAvatarCustomization( bytes );

            var asset = ArtPacks[ data.PackIndex ].Categories[ data.CategoryIndex ].Assets[ data.AssetIndex ];
            asset.Category = ArtPacks[ data.PackIndex ].Categories[ data.CategoryIndex ];
            var defaultObjectData = CreateCustomizationObjectData( data.PackIndex, data.CategoryIndex, data.AssetIndex, asset );

            for( int i = 0; i < Mathf.Min( data.ColorValues.Length, defaultObjectData.ColorValues.Length ); ++i )
            {
                defaultObjectData.ColorValues[ i ] = data.ColorValues[ i ];
            }

            for( int i = 0; i < Mathf.Min( data.FloatValues.Length, defaultObjectData.FloatValues.Length ); ++i )
            {
                defaultObjectData.FloatValues[ i ] = data.FloatValues[ i ];
            }

            //Debug.Log( "Des: " + data.AssetIndex + " - " + data.CategoryIndex + " - colors: " + data.ColorValues.Length + " - " + serializedValue );
            SetCustomizationObject( defaultObjectData, false, false );
        }

        string SerializeProperty( CustomizationArtCategory.CategoryType type )
        {
            if( CurrentCustomizationObjects.ContainsKey( type ) )
            {
                var bytes = CustomTypes.SerializeAvatarCustomization( CurrentCustomizationObjects[ type ] );

                string value = System.Convert.ToBase64String( bytes );

                //Debug.Log( "Ser: " + CurrentCustomizationObjects[ type ].AssetIndex + " - " + CurrentCustomizationObjects[ type ].CategoryIndex + " - colors: " + CurrentCustomizationObjects[ type ].ColorValues.Length + " - " + value );

                return value;
            }

            return "";
        }

        public void ApplyCustomization( AvatarCustomization avatar )
        {
            //avatar.Device = GetDeviceType();
            avatar.ApplyCustomization();

            var badges = avatar.GetComponentsInChildren<AvatarBadge>();
            foreach( var badge in badges )
            {
                badge.NickName = Nickname;
                badge.CompanyName = CompanyName;
                badge.ApplyNames();
            }
        }

        public ExitGames.Client.Photon.Hashtable GetCustomizationAsHashtable()
        {
            var result = new ExitGames.Client.Photon.Hashtable();

            result.Add( AvatarDeviceIcons.ClientDeviceTypePropertyName, (int)GetDeviceType() );
            result.Add( ConferenceCustomProperties.NickNamePropertyName, Nickname );
            result.Add( ConferenceCustomProperties.CompanyNamePropertyName, CompanyName );

            foreach( var pair in CurrentCustomizationObjects )
            {
                result.Add( AvatarCustomization.GetKeyFromCategoryType( pair.Key ), pair.Value );
            }

            return result;
        }

        AvatarDeviceIcons.DeviceType GetDeviceType()
        {
            
            AvatarDeviceIcons.DeviceType result = ViewModeManager.Instance.CurrentViewMode.GetViewMode() == ViewModeManager.ViewMode.XR ? AvatarDeviceIcons.DeviceType.XR : AvatarDeviceIcons.DeviceType.PC;
# if UNITY_WEBGL 
            result = AvatarDeviceIcons.DeviceType.WEB;
#endif
            return result;
        }

    }
}