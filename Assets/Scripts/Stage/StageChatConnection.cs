using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceHub.Conference
{


    public class StageChatConnection : MonoBehaviour, IChatClientListener
    {
        public const string EmoteChannelName = "StageEmote";
        public const string SpeakerXRChannelName = "SpeakerXR";
        public const string SpeakerXRCustomizationChannelName = "SpeakerXRCustomization";
        public const string PresentationChannelName = "Presentation";
        public const string TimerChannelName = "TimerStarted";
        public const string SpeakerPingChannelName = "SpeakerPing";

        public static string Nickname { get; private set; }
        ChatClient m_Client;
        public ChatAppSettings Settings;


        public UnityAction<string, string[], object[]> OnGetMessagesCallback;
        ChatChannel m_EmoteChannel = null;

        void Start()
        {
            Debug.Log( "Chat Client Connecting..." );

            CustomTypes.Register();

            m_Client = new ChatClient( this );
            m_Client.ChatRegion = Settings.FixedRegion;

            Nickname = CustomizationData.Instance.Nickname + ", " + CustomizationData.Instance.CompanyName + "," + System.Guid.NewGuid().ToString();
            m_Client.Connect( Settings.AppId, VersionData.AppVersion, new AuthenticationValues( Nickname ) );
        }

        private void OnDestroy()
        {
            m_Client.Disconnect();
        }


        void Update()
        {
            m_Client.Service();
            if( m_EmoteChannel == null )
            {
                m_Client.TryGetChannel( EmoteChannelName, out m_EmoteChannel );
            }
        }

        public bool IsConnected()
        {
            return m_Client != null && m_Client.State == ChatState.ConnectedToFrontEnd;
        }

        public int GetEmoteChannelUserCount()
        {
            if( m_EmoteChannel == null )
            {
                return -1;
            }

            return m_EmoteChannel.Subscribers.Count;
        }

        public void SendSpeakerCustomization( Hashtable hashtable )
        {
            m_Client.PublishMessage( SpeakerXRCustomizationChannelName, hashtable );
        }

        public void SendEmote( byte message )
        {
            m_Client.PublishMessage( EmoteChannelName, message );
        }

        public void SpeakerSendPing()
        {
            m_Client.PublishMessage( SpeakerPingChannelName, (byte)1 );
        }

        public void SendPresentationUri( string uri )
        {
            m_Client.PublishMessage( PresentationChannelName, uri );
        }

        public void UpdateTimer( int timer ){
             m_Client.PublishMessage( TimerChannelName, timer );
        }

        public void SendSpeakerXRUpdate( Hashtable hashtable )
        {
            m_Client.PublishMessage( SpeakerXRChannelName, hashtable );
        }

        public void DebugReturn( DebugLevel level, string message )
        {
            Debug.Log( message );
        }

        public void OnChatStateChange( ChatState state )
        {
            Debug.Log( "OnChatStateChange " + state );
        }

        public void OnConnected()
        {
            Debug.Log( "Chat Client Connected" );
            m_Client.Subscribe( EmoteChannelName, 0, 0, new ChannelCreationOptions() { PublishSubscribers = true } );
            m_Client.Subscribe( SpeakerXRChannelName, 0, 1 );
            m_Client.Subscribe( SpeakerPingChannelName, 0, 0 );
            m_Client.Subscribe( SpeakerXRCustomizationChannelName, 0, 1 );
            m_Client.Subscribe( PresentationChannelName, 0, 1 );
            m_Client.Subscribe( TimerChannelName, 0, 1 );
        }

        public void OnDisconnected()
        {
            Debug.Log( "Chat Client Disconnected" );
        }

        public void OnGetMessages( string channelName, string[] senders, object[] messages )
        {
            OnGetMessagesCallback?.Invoke( channelName, senders, messages );
        }

        public void OnPrivateMessage( string sender, object message, string channelName )
        {
        }

        public void OnStatusUpdate( string user, int status, bool gotMessage, object message )
        {
            Debug.Log( "Chat Client OnStatusUpdate" );
        }

        public void OnSubscribed( string[] channels, bool[] results )
        {
            Debug.Log( "Chat Client OnSubscribed" );
        }

        public void OnUnsubscribed( string[] channels )
        {
            Debug.Log( "Chat Client OnUnsubscribed" );
        }

        public void OnUserSubscribed( string channel, string user )
        {
            Debug.Log( "Chat Client OnUserSubscribed " + channel + ", " + user );
        }

        public void OnUserUnsubscribed( string channel, string user )
        {
            Debug.Log( "Chat Client OnUserUnsubscribed " + channel + ", " + user );
        }

        public void OnChannelPropertiesChanged( string channel, string userId, System.Collections.Generic.Dictionary<object, object> properties ) { }
        public void OnUserPropertiesChanged( string channel, string targetUserId, string senderUserId, System.Collections.Generic.Dictionary<object, object> properties ) { }
        public void OnErrorInfo( string channel, string error, object data ) { }
    }
}