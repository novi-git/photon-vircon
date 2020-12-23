using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public static class PhotonPlayerExtensions
    {
        public static string GetNickNameProperty( this Player player )
        {
            return player.GetStringProperty( ConferenceCustomProperties.NickNamePropertyName );
        }

        public static string GetCompanyNameProperty( this Player player )
        {
            return player.GetStringProperty( ConferenceCustomProperties.CompanyNamePropertyName );
        }

        public static ConferenceCustomProperties.CallStatusType GetCallStatusProperty( this Player player )
        {
            if( player.CustomProperties == null || player.CustomProperties.ContainsKey( ConferenceCustomProperties.CallStatusPropertyName ) == false )
            {
                return ConferenceCustomProperties.CallStatusType.NotAvailable;
            }

            return (ConferenceCustomProperties.CallStatusType)player.CustomProperties[ ConferenceCustomProperties.CallStatusPropertyName ];
        }

        public static bool GetInCallProperty( this Player player )
        {
            if( player.CustomProperties.ContainsKey( ConferenceCustomProperties.InCallPropertyName ) == false )
            {
                return false;
            }

            return (bool)player.CustomProperties[ ConferenceCustomProperties.InCallPropertyName ];
        }

        public static int GetVoiceActorNumberProperty( this Player player )
        {
            if( player.CustomProperties.ContainsKey( ConferenceCustomProperties.VoiceActorNumberPropertyName ) == false )
            {
                return -1;
            }

            return (int)player.CustomProperties[ ConferenceCustomProperties.VoiceActorNumberPropertyName ];
        }

        public static string GetStringProperty( this Player player, string propertyName )
        {
            if( player == null )
            {
                return "";
            }

            if( player.CustomProperties.ContainsKey( propertyName ) == false )
            {
                return "";
            }

            return (string)player.CustomProperties[ propertyName ];
        }
    }

    public class ConferenceCustomProperties : MonoBehaviour
    {
        public const string NickNamePropertyName = "name";
        public const string CompanyNamePropertyName = "CompanyName";
        public const string CallStatusPropertyName = "callstatus";
        public const string InCallPropertyName = "incall";
        public const string VoiceActorNumberPropertyName = "voicenumber";

        public enum CallStatusType : byte
        {
            NotAvailable,
            Available,
            Busy,
            DoNotDisturb,
        }

        public Player Player { get; set; }

        UnityAction<ExitGames.Client.Photon.Hashtable> m_PlayerPropertiesChangedCallback;


        public void RegisterPlayerPropertiesChangedCallback( UnityAction<ExitGames.Client.Photon.Hashtable> callback )
        {
            m_PlayerPropertiesChangedCallback -= callback;
            m_PlayerPropertiesChangedCallback += callback;
            if ( Player != null )
            {
                callback( Player.CustomProperties );
            }
        }

        public void UnregisterPlayerPropertiesChangedCallback( UnityAction<ExitGames.Client.Photon.Hashtable> callback )
        {
            m_PlayerPropertiesChangedCallback -= callback;
        }


        public void OnPlayerPropertiesChanged( ExitGames.Client.Photon.Hashtable changedProperties )
        {
            Debug.Log( "Custom properties changed " + changedProperties.Count, this );
            m_PlayerPropertiesChangedCallback?.Invoke( changedProperties );
        }

        public string NickName
        {
            get
            {
                return Player.GetNickNameProperty();
            }
        }

        public string CompanyName
        {
            get
            {
                return Player.GetCompanyNameProperty();
            }
        }

        public CallStatusType CallStatus
        {
            get
            {
                if( Player == null )
                {
                    return CallStatusType.NotAvailable;
                }

                return Player.GetCallStatusProperty();
            }
            set
            {
                SetCustomProperty( CallStatusPropertyName, (byte)value );
            }
        }

        public bool IsInCall
        {
            get
            {
                return Player.GetInCallProperty();
            }
            set
            {
                SetCustomProperty( InCallPropertyName, value );
            }
        }

        public int VoiceActorNumber
        {
            get
            {
                return Player.GetVoiceActorNumberProperty();
            }
            set
            {
                SetCustomProperty( VoiceActorNumberPropertyName, value );
            }
        }

        public bool SetCustomProperty( string name, byte value )
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add( name, value );

            return SetCustomProperties( properties );
        }

        public bool SetCustomProperty( string name, int value )
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add( name, value );

            return SetCustomProperties( properties );
        }

        public bool SetCustomProperty( string name, bool value )
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add( name, value );

            return SetCustomProperties( properties );
        }

        public bool SetCustomProperties( ExitGames.Client.Photon.Hashtable propertiesToSet, ExitGames.Client.Photon.Hashtable expectedValues = null, WebFlags webFlags = null )
        {
            return Player.SetCustomProperties( propertiesToSet, expectedValues, webFlags );
        }
    }
}