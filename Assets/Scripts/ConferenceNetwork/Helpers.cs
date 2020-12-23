using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using UnityEngine.AI;

namespace SpaceHub
{
    namespace Conference
    {
        public class Helpers
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            [DllImport( "__Internal" )]
            private static extern void openURLInNewTab( string url );
#endif
            public static void OpenURL( string url )
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                openURLInNewTab( url );
#else
                Application.OpenURL( url );
#endif
            }
            public static byte GetHighFrequencyGroupId( Vector3 position )
            {
                Vector3 areaSize = new Vector3( 64, 16, 64 );
                int separations = 3;

                if( ConferenceSceneSettings.Instance != null )
                {
                    areaSize.x = ConferenceSceneSettings.Instance.AreaWidth;
                    areaSize.z = ConferenceSceneSettings.Instance.AreaLength;
                    separations = ConferenceSceneSettings.Instance.HighFrequencyGroupSeparations;
                }

                var areaHalfSize = areaSize * 0.5f;

                position = ClampToBoothSize( position, areaSize, areaHalfSize );

                int x = (int)( ( ( position.x + areaHalfSize.x ) / areaSize.x ) * separations );
                int z = (int)( ( ( position.z + areaHalfSize.z ) / areaSize.z ) * separations );

                return (byte)( z * separations + x + 2 ) ;
            }

            public static Vector3 ClampToBoothSize( Vector3 position )
            {
                Vector3 areaSize = new Vector3( 64, 16, 64 );

                if( ConferenceSceneSettings.Instance != null )
                {
                    areaSize.x = ConferenceSceneSettings.Instance.AreaWidth;
                    areaSize.y = ConferenceSceneSettings.Instance.AreaHeight;
                    areaSize.z = ConferenceSceneSettings.Instance.AreaLength;
                }

                var areaHalfSize = areaSize * 0.5f;

                return ClampToBoothSize( position, areaSize, areaHalfSize );
            }

            public static Vector3 ClampToBoothSize( Vector3 position, Vector3 areaSize, Vector3 areaHalfSize )
            {
                position.x = Mathf.Clamp( position.x, -areaHalfSize.x, areaHalfSize.x );
                position.y = Mathf.Clamp( position.y, -areaHalfSize.y, areaHalfSize.y );
                position.z = Mathf.Clamp( position.z, -areaHalfSize.z, areaHalfSize.z );

                return position;
            }

            public static void Vector3ToExpoPosition( Vector3 position, ref byte[] output )
            {
                Vector3 areaSize = new Vector3( 64, 16, 64 );

                if( ConferenceSceneSettings.Instance != null )
                {
                    areaSize.x = ConferenceSceneSettings.Instance.AreaWidth;
                    areaSize.y = ConferenceSceneSettings.Instance.AreaHeight;
                    areaSize.z = ConferenceSceneSettings.Instance.AreaLength;
                }

                var areaHalfSize = areaSize * 0.5f;

                position = ClampToBoothSize( position, areaSize, areaHalfSize );

                var crunchedPosition = areaSize * ( 1f / 256f );

                output[ 0 ] = (byte)( Mathf.RoundToInt( position.x * crunchedPosition.x ) + 128 );
                output[ 1 ] = (byte)( Mathf.RoundToInt( position.y * crunchedPosition.y ) + 128 );
                output[ 2 ] = (byte)( Mathf.RoundToInt( position.z * crunchedPosition.z ) + 128 );
            }

            public static byte[] Vector3ToExpoPosition( Vector3 position )
            {
                byte[] output = new byte[ 3 ];

                Vector3ToExpoPosition( position, ref output );

                return output;
            }

            public static Vector3 ExpoPositionToVector3( byte[] position )
            {
                Vector3 areaSize = new Vector3( 64, 16, 64 );

                if( ConferenceSceneSettings.Instance != null )
                {
                    areaSize.x = ConferenceSceneSettings.Instance.AreaWidth;
                    areaSize.y = ConferenceSceneSettings.Instance.AreaHeight;
                    areaSize.z = ConferenceSceneSettings.Instance.AreaLength;
                }

                var crunchedPosition = areaSize * ( 1f / 256f );

                Vector3 output = Vector3.zero;
                output.x = ( position[ 0 ] - 128 ) / crunchedPosition.x;
                output.y = ( position[ 1 ] - 128 ) / crunchedPosition.y;
                output.z = ( position[ 2 ] - 128 ) / crunchedPosition.z;

                output.x += Random.Range( -0.2f, 0.2f );
                output.z += Random.Range( -0.2f, 0.2f );

                return output;
            }

            public static byte RotationToByte( Quaternion rotation )
            {
                return (byte)Mathf.RoundToInt( Mathf.Repeat( rotation.eulerAngles.y, 360f ) / 360f * 255f );
            }

            public static Quaternion ByteToRotation( byte rotation )
            {
                return Quaternion.Euler( 0, (float)rotation / 255f * 360f, 0 );
            }

            public static void DestroyAllChildren( Transform parent )
            {
                if( parent == null )
                {
                    return;
                }

                int childCount = parent.childCount;
                for( int i = 0; i < childCount; ++i )
                {
                    GameObject.Destroy( parent.GetChild( i ).gameObject );
                }
            }


            public static float AngleSigned( Vector3 v1, Vector3 v2, Vector3 n )
            {
                return Mathf.Atan2(
                    Vector3.Dot( n, Vector3.Cross( v1, v2 ) ),
                    Vector3.Dot( v1, v2 ) ) * Mathf.Rad2Deg;
            }

            public static Quaternion Damp( Quaternion from, Quaternion to, float smoothing )
            {
                return Quaternion.Slerp( from, to, 1f - Mathf.Exp( -smoothing * Time.unscaledDeltaTime ) );
            }

            public static Vector3 Damp( Vector3 from, Vector3 to, float smoothing )
            {
                return Vector3.Lerp( from, to, 1f - Mathf.Exp( -smoothing * Time.unscaledDeltaTime ) );
            }

            public static Vector3 SDamp( Vector3 from, Vector3 to, float smoothing )
            {
                return Vector3.Slerp( from, to, 1f - Mathf.Exp( -smoothing * Time.unscaledDeltaTime ) );
            }

            public static float Damp( float from, float to, float smoothing )
            {
                return Mathf.Lerp( from, to, 1f - Mathf.Exp( -smoothing * Time.unscaledDeltaTime ) );
            }

            public static Color Damp( Color from, Color to, float smoothing )
            {
                return Color.Lerp( from, to, 1f - Mathf.Exp( -smoothing * Time.unscaledDeltaTime ) );
            }

            public static IEnumerator DownloadTextureRoutine( string uri, UnityAction<Texture2D> callback, bool generateMipmaps = true )
            {
                using( var request = UnityWebRequestTexture.GetTexture( uri ) )
                {
                    yield return request.SendWebRequest();

                    if( request.isNetworkError || request.isHttpError )
                    {
                        Debug.LogError( request.error );
                    }
                    else
                    {
                        var wwwTexture = DownloadHandlerTexture.GetContent( request );

                        if( generateMipmaps )
                        {
                            var mipMapTexture = new Texture2D( wwwTexture.width, wwwTexture.height, wwwTexture.format, true );
                            mipMapTexture.SetPixels( wwwTexture.GetPixels( 0 ) );
                            mipMapTexture.Apply( true );
                            callback?.Invoke( mipMapTexture );
                        }
                        else
                        {
                            callback?.Invoke( wwwTexture);
                        }
                    }
                }
            }
        }
    }
}