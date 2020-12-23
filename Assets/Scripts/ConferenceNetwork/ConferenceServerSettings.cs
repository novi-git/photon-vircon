using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class ConferenceServerSettings : MonoBehaviour
    {
        public const string RealtimeAppIdKey = "RealtimeAppId";
        public const string VoiceAppIdKey = "VoiceAppId";
        public const string ChatAppIdKey = "ChatAppId";
        public const string ApiBaseUrlKey = "ApiBaseUrl";

        public static ConferenceServerSettings Instance;

        [HideInInspector]
        public bool BuildMode = false;
        public bool StoreAppIdsInVersionControl = false;
        public string DefaultPhotonRegion = "eu";

        [HideInInspector]
        public string RealtimeAppId;

        [HideInInspector]
        public string VoiceAppId;

        [HideInInspector]
        public string ChatAppId;

        [HideInInspector]
        public string ApiBaseUrl;

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

        void Update(){
           //
        }

        public string GetAppVersion()
        {
            return VersionData.AppVersion;
        }

        public string GetRealtimeAppId()
        {
            if( BuildMode || StoreAppIdsInVersionControl )
            {
                return RealtimeAppId;
            }
            else
            {
                return PlayerPrefs.GetString( RealtimeAppIdKey );
            }
        }

        public string GetVoiceAppId()
        {
            if( BuildMode || StoreAppIdsInVersionControl )
            {
                return VoiceAppId;
            }
            else
            {
                return PlayerPrefs.GetString( VoiceAppIdKey );
            }
        }

        public string GetChatAppId()
        {
            if( BuildMode || StoreAppIdsInVersionControl )
            {
                return ChatAppId;
            }
            else
            {
                return PlayerPrefs.GetString( ChatAppIdKey );
            }
        }

        public string GetApiBaseUrl()
        {
            if( BuildMode || StoreAppIdsInVersionControl )
            {
                return ApiBaseUrl;
            }
            else
            {
                return PlayerPrefs.GetString( ApiBaseUrlKey );
            }
        }

        public string GetApiRoute( string route )
        {
            return Path.Combine( GetApiBaseUrl(), route ).Replace( "\\", "/" );
        }
              

        public void EnableBuildMode()
        {
            BuildMode = true;
            RealtimeAppId = PlayerPrefs.GetString( RealtimeAppIdKey );
            VoiceAppId = PlayerPrefs.GetString( VoiceAppIdKey );
            ChatAppId = PlayerPrefs.GetString( ChatAppIdKey );
            ApiBaseUrl = PlayerPrefs.GetString( ApiBaseUrlKey );
        }
    }
}
