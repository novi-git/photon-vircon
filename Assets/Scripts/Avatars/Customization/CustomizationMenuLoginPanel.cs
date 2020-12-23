using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using SpaceHub.SimpleJSON;
using System;

namespace SpaceHub.Conference
{
    public class CustomizationMenuLoginPanel : MonoBehaviour
    {
        public enum SpaceHubLoginStage
        {
            Email,
            Verifying,
            Authenticated,
            Guest,
            INVALID = 255,
        }

        public GameObject LoginParent;
        public GameObject VerifyingParent;
        public GameObject AuthenticatedParent;
        public GameObject GuestParent;

        public TextMeshProUGUI VerifyingText;
        public TextMeshProUGUI AuthenticatedText;

        public TMP_InputField LoginEmailField;
        public TMP_InputField GuestNameField;

        public string SpaceHubLoginEmail
        {
            get
            {
                return PlayerPrefs.GetString( "SpaceHubLoginEmail" );
            }
        }

        public string SpaceHubLoginToken
        {
            get
            {
                return PlayerPrefs.GetString( "SpaceHubLoginToken" );
            }
        }

        public bool SpaceHubLoginIsVerified
        {
            get
            {
                return PlayerPrefs.GetInt( "SpaceHubLoginIsVerified" ) == 1;
            }
        }

        SpaceHubLoginStage m_LoginStage = SpaceHubLoginStage.INVALID;
        float m_VerifyingStartTime;
        float m_LastTokenLoginTime;

        private void Awake()
        {
            GetComponentInParent<UIFunnelScreen>().ShowCallback += OnShow;
        }

        void OnShow()
        {
            if( m_LoginStage == SpaceHubLoginStage.INVALID )
            {
                SetLoginStage( SpaceHubLoginStage.Email );
            }
        }

        private void Start()
        {
            LoginEmailField.SetTextWithoutNotify( PlayerPrefs.GetString( "LoginMail" ) );
            GuestNameField.SetTextWithoutNotify( PlayerPrefs.GetString( ConferenceCustomProperties.NickNamePropertyName ) );

            if( string.IsNullOrEmpty( SpaceHubLoginEmail ) == false &&
                string.IsNullOrEmpty( SpaceHubLoginToken ) == false )
            {
                if( SpaceHubLoginIsVerified == true )
                {
                    SetLoginStage( SpaceHubLoginStage.Authenticated );
                }
                else
                {
                    SendTokenLoginRequest();
                }
            }
            else
            {
                SetLoginStage( SpaceHubLoginStage.Guest );
            }

            string customAuthenticationFailedMessage = PlayerPrefs.GetString( "OnCustomAuthenticationFailedMessage" );

            if( string.IsNullOrEmpty( customAuthenticationFailedMessage ) == false )
            {
                MessagePopup.Show( "Login error: " + customAuthenticationFailedMessage, LogType.Warning );
                PlayerPrefs.SetString( "OnCustomAuthenticationFailedMessage", "" );
            }

#if UNITY_WEBGL || UNITY_ANDROID || UNITY_EDITOR
            SetLoginStage( SpaceHubLoginStage.Guest ); 
#endif

        }

        private void Update()
        {
            if( m_LoginStage == SpaceHubLoginStage.Verifying )
            {
                UpdateVerifyingStage();
            }
        }

        void UpdateVerifyingStage()
        {
            float timeElapsed = Time.realtimeSinceStartup - m_VerifyingStartTime;

            if( timeElapsed > 5 * 60f )
            {
                SetLoginStage( SpaceHubLoginStage.Email );
                MessagePopup.Show( "Failed to verify your login." );
                return;
            }

            float timeSinceLastVerifyCheck = Time.realtimeSinceStartup - m_LastTokenLoginTime;

            //Check every 5 seconds if the account has been verified
            if( timeSinceLastVerifyCheck < 5f )
            {
                return;
            }

            SendTokenLoginRequest();
        }

        private void OnApplicationFocus( bool focus )
        {
            if( m_LoginStage != SpaceHubLoginStage.Verifying )
            {
                return;
            }

            if( focus == true )
            {
                SendTokenLoginRequest();
            }
        }

        public void ToggleGuest()
        {
            if( m_LoginStage == SpaceHubLoginStage.Guest )
            {
                SetLoginStage( SpaceHubLoginStage.Email );
            }
            else
            {
                SetLoginStage( SpaceHubLoginStage.Guest );
            }
        }

        void SetLoginStage( SpaceHubLoginStage loginStage )
        {
            if( loginStage == m_LoginStage )
            {
                return;
            }

            m_LoginStage = loginStage;

            LoginParent.SetActive( loginStage == SpaceHubLoginStage.Email );
            VerifyingParent.SetActive( loginStage == SpaceHubLoginStage.Verifying );
            AuthenticatedParent.SetActive( loginStage == SpaceHubLoginStage.Authenticated );
            GuestParent.SetActive( loginStage == SpaceHubLoginStage.Guest );

             Debug.Log("Login Stage " + loginStage);

            if( loginStage == SpaceHubLoginStage.Verifying )
            {
                m_VerifyingStartTime = Time.realtimeSinceStartup;
                VerifyingText.text = $"Verifying {SpaceHubLoginEmail}...";
            }
            else if( loginStage == SpaceHubLoginStage.Authenticated )
            {
                AuthenticatedText.text = "Logged in as\n" + SpaceHubLoginEmail;
            }
        }

        public void Register()
        {
            Helpers.OpenURL( "https://spacehub.world" );
        }

        public void Login()
        {
            if( string.IsNullOrWhiteSpace( LoginEmailField.text ) )
            {
                MessagePopup.Show( "Please enter a valid email", LogType.Warning );
            }
            else
            {
                if( PlayerPrefs.GetString( "SpaceHubLoginEmail2" ) == LoginEmailField.text && string.IsNullOrEmpty( SpaceHubLoginToken ) == false )
                {
                    //Already has a token saved for this e-mail, trying to login with existing token
                    PlayerPrefs.SetString( "SpaceHubLoginEmail", LoginEmailField.text );
                }
                else
                {
                    PlayerPrefs.SetString( "SpaceHubLoginEmail", LoginEmailField.text );
                    PlayerPrefs.SetString( "SpaceHubLoginEmail2", LoginEmailField.text );
                    PlayerPrefs.SetString( "SpaceHubLoginToken", Guid.NewGuid().ToString() );
                }

                SendTokenLoginRequest();
            }
        }

        public void Logout()
        {
            PlayerPrefs.SetInt( "SpaceHubLoginIsVerified", 0 );
            PlayerPrefs.SetString( "SpaceHubLoginEmail", "" );
            SetLoginStage( SpaceHubLoginStage.Email );

            SetNames( GuestNameField.text, "Guest" );
        }

        void SendTokenLoginRequest()
        {
            SetLoginStage( SpaceHubLoginStage.Verifying );
            m_LastTokenLoginTime = Time.realtimeSinceStartup;

            StartCoroutine( SendTokenLoginRequestRoutine() );
        }

        IEnumerator SendTokenLoginRequestRoutine()
        {
            string email = PlayerPrefs.GetString( "SpaceHubLoginEmail" );
            string token = PlayerPrefs.GetString( "SpaceHubLoginToken" );

            if( email == "" )
            {
                Debug.LogError( "No login email stored" );
                yield break;
            }

            if( token == "" )
            {
                Debug.LogError( "No login token stored" );
                yield break;
            }

            WWWForm formData = new WWWForm();
            formData.AddField( "email", email );
            formData.AddField( "token", token );

            UnityWebRequest request = UnityWebRequest.Post( ConferenceServerSettings.Instance.GetApiRoute( "v1/photon/login" ), formData );

            yield return request.SendWebRequest();

            if( request.isNetworkError )
            {
                Debug.LogWarning( "Login error: " + request.error );
            }
            else
            {
                Debug.Log( "Received: " + request.downloadHandler.text );

                //If the tokens are different the user has cancelled/logged out
                if( token == PlayerPrefs.GetString( "SpaceHubLoginToken" ) )
                {
                    JSONNode node = JSON.Parse( request.downloadHandler.text );

                    if( node[ "ResultCode" ] != null )
                    {
                        switch( node[ "ResultCode" ].AsInt )
                        {
                        case 0: //Login successful
                            SetNames( node[ "Nickname" ], node[ "CompanyName" ] );

                            PlayerPrefs.SetInt( "SpaceHubLoginIsVerified", 1 );
                            SetLoginStage( SpaceHubLoginStage.Authenticated );
                            GetComponentInParent<UiFunnelManager>()?.Next();
                            break;
                        case 1: //New verification email sent
                            SetLoginStage( SpaceHubLoginStage.Verifying );
                            MessagePopup.Show( "Please click the link in your email to verify your login." );
                            break;
                        case 2: //Token not verified yet
                            SetLoginStage( SpaceHubLoginStage.Verifying );
                            break;
                        default: //Error
                            Logout();
                            MessagePopup.Show( node[ "Message" ], LogType.Warning );
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log( "Received web response, but tokens are different. Ignoring..." );
                }
            }
        }

        public void OnLoginNameChanged( string value )
        {
            PlayerPrefs.SetString( "LoginMail", LoginEmailField.text );

            GuestNameField.SetTextWithoutNotify( "" );

            SetNames( "", "" );
        }

        public void OnGuestNameChanged( string value )
        {
            LoginEmailField.SetTextWithoutNotify( "" );
            PlayerPrefs.SetString( "LoginMail", "" );

            SetNames( GuestNameField.text, "Guest" );
            //m_Menu.SetName( GuestNameField.text );
            //m_Menu.SetCompany( "Guest" );
        }

        public void SetNames( string nickname, string company )
        {
            Debug.Log( $"Setting names: {nickname}, {company}" );
            CustomizationData.Instance.Nickname = nickname;
            CustomizationData.Instance.CompanyName = company;
            CustomizationData.Instance.SaveToPlayerPrefs();
        }

    }
}