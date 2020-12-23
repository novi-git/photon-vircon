using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.Networking;

namespace SpaceHub.Conference
{
    [Serializable]
    public class VersionData
    {
        public string Name;
        public string AppName;
        public int Version;
        public int VersionMinor;
        public int BuildNumber;
        public string ReleaseDate;
        public string BuildDate;

        public static string AppVersion = "0.4";
        public static string FullAppVersion = "0.4";
        public static VersionData Current;

        public static void LoadImmediate()
        {
            string path = Path.Combine( Application.streamingAssetsPath, "deployConfig.json" );

            if( File.Exists( path ) == false )
            {
                return;
            }

            LoadCurrentFromJson( File.ReadAllText( path ) );
        }

        public static IEnumerator Load()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            LoadImmediate();
            yield break;
#else
            string path = Path.Combine( Application.streamingAssetsPath, "deployConfig.json" );
            string versionJson = "";

            Debug.Log( "Load version data from " + path );

            using( UnityWebRequest webRequest = UnityWebRequest.Get( path ) )
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if( webRequest.isNetworkError )
                {
                    Debug.Log( "WebRequest Error: " + webRequest.error );
                }
                else
                {

                    versionJson = webRequest.downloadHandler.text;
                }
            }

            LoadCurrentFromJson( versionJson );
#endif
        }

        static void LoadCurrentFromJson( string json )
        {
            Current = JsonUtility.FromJson<VersionData>( json );

            AppVersion = Current.Version + "." + Current.VersionMinor;
            FullAppVersion = AppVersion + "." + Current.BuildNumber;
        }

        public string GetVersion( bool includeDate )
        {
            if( includeDate == true )
            {
                return Version + "." + VersionMinor + "." + BuildNumber + " (" + BuildDate + ")";
            }

            return Version + "." + VersionMinor + "." + BuildNumber;
        }

        public override string ToString()
        {
            return GetVersion( true );
        }
    }

    [RequireComponent( typeof( TextMeshProUGUI  ) )]
    public class VersionText : MonoBehaviour
    {
        public string Prefix;

        IEnumerator Start()
        {
            if( VersionData.Current == null )
            {
                StartCoroutine( VersionData.Load() );
                GetComponent<TextMeshProUGUI>().text = "-";

                while( VersionData.Current == null )
                {
                    yield return null;
                }
            }

            GetComponent<TextMeshProUGUI>().text = Prefix + VersionData.Current.ToString();
        }
    }
}