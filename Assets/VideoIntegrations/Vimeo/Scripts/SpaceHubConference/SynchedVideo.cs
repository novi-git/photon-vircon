using ExitGames.Client.Photon;
using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vimeo.Player;

namespace SpaceHub.Conference
{
    [RequireComponent( typeof( VimeoPlayer ), typeof( MediaPlayer ) )]
    public class SynchedVideo : MonoBehaviour
    {
        public const string VideoSyncProperty = "VimeoVideoStartTime";
        public string VimeoUrl;
        public TextMeshPro DebugTimecode;

        VimeoPlayer m_VimeoPlayer;
        MediaPlayer m_MediaPlayer;

        // Start is called before the first frame update
        void Awake()
        {
            m_VimeoPlayer = GetComponent<VimeoPlayer>();
            m_MediaPlayer = GetComponent<MediaPlayer>();
        }

        private IEnumerator Start()
        {
            yield return null;

            m_VimeoPlayer.OnVideoMetadataLoad += OnVideoLoaded;
            m_VimeoPlayer.LoadVideo( VimeoUrl );
        }

        private void Update()
        {
            if( m_MediaPlayer.Control.IsPlaying() == true && DebugTimecode != null )
            {
                DebugTimecode.text = ( m_MediaPlayer.Control.GetCurrentTimeMs() / 1000 ).ToString( "0.0" );
            }
        }

        IEnumerator SyncPlaybackRoutine()
        {
            while( PlayerLocal.Instance.Client.InRoom == false )
            {
                yield return null;
            }

            if( PlayerLocal.Instance.Client.CurrentRoom.CustomProperties.ContainsKey( VideoSyncProperty ) == false )
            {
                var properties = new ExitGames.Client.Photon.Hashtable();
                properties.Add( VideoSyncProperty, PlayerLocal.Instance.Client.LoadBalancingPeer.ServerTimeInMilliSeconds );

                PlayerLocal.Instance.Client.CurrentRoom.SetCustomProperties( properties );
            }

            while( PlayerLocal.Instance.Client.CurrentRoom.CustomProperties.ContainsKey( VideoSyncProperty ) == false )
            {
                yield return null;
            }

            StartCoroutine( PlayRoutine() );
        }

        void OnVideoLoaded()
        {
            if( PlayerLocal.Instance == null )
            {
                StartCoroutine( PlayRoutine() );
            }
            else
            {
                StartCoroutine( SyncPlaybackRoutine() );
            }
        }

        IEnumerator PlayRoutine()
        {
            Debug.Log( "StartPlayRoutine()" );
            m_VimeoPlayer.Play();
            m_MediaPlayer.Play();
            m_MediaPlayer.Control.MuteAudio( true );
            /*while( m_MediaPlayer.Control.IsPlaying() == false )
            {
                Debug.Log( $"Playing: {m_MediaPlayer.Control.IsPlaying()}. - Buff: {m_MediaPlayer.Control.IsBuffering()}. - Seek: {m_MediaPlayer.Control.IsSeeking()} " );
                yield return null;
            }*/

            yield return new WaitForSeconds( 0.5f );

            m_MediaPlayer.Stop();
            yield return new WaitForSeconds( 0.5f );

            float timeMs = 0;

            if( PlayerLocal.Instance != null &&
                PlayerLocal.Instance.Client != null &&
                PlayerLocal.Instance.Client.LoadBalancingPeer != null &&
                PlayerLocal.Instance.Client.CurrentRoom.CustomProperties.ContainsKey( VideoSyncProperty ) )
            {
                timeMs = PlayerLocal.Instance.Client.LoadBalancingPeer.ServerTimeInMilliSeconds - (int)PlayerLocal.Instance.Client.CurrentRoom.CustomProperties[ VideoSyncProperty ];
            }

            m_MediaPlayer.Control.MuteAudio( false );
            m_MediaPlayer.Control.Seek( timeMs );
            m_MediaPlayer.Play();
        }
    }
}