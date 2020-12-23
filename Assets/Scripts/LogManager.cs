using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Events;
using SpaceHub.Conference;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;
    StringBuilder m_OutputSb = new StringBuilder();

    public UnityAction LogChangedCallback;
    public int LogsHandled = 0;

    public static bool ShowWarningPopup = false;

    string[] m_DisplayBuffer = new string[13];
    string[] m_StoreForSendBuffer = new string[ 50 ];

    int m_BufferIndex = 0;
    int m_StoreBufferIndex = 0;

    float m_LastSendErrorLogTime = -60;
    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        Debug.Log( "Instancing LogManager" );
        if( Instance == null )
        {
            var go = new GameObject( "LogManager", typeof( LogManager ) );
            DontDestroyOnLoad( go );
        }
    }

    private void Awake()
    {
        Instance = this;
        Application.logMessageReceived += HandleLog;
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public string GetLastLogs()
    {
        m_OutputSb.Clear();
        for( int i = 0; i< m_DisplayBuffer.Length; ++i )
        {
            string line = m_DisplayBuffer[ ( i + m_BufferIndex ) % m_DisplayBuffer.Length ];

            if( string.IsNullOrWhiteSpace( line ) == false )
            {
                m_OutputSb.AppendLine( m_DisplayBuffer[ ( i + m_BufferIndex ) % m_DisplayBuffer.Length ] );
            }
        }
        return m_OutputSb.ToString();
    }

    public void SendLogToDeveloper()
    {
        float timeSinceLastLog = Time.realtimeSinceStartup - m_LastSendErrorLogTime;
        if( timeSinceLastLog < 30f )
        {
            MessagePopup.Show( $"Please wait {(30f - timeSinceLastLog).ToString( "0" )} seconds until you can send a new error log.", LogType.Warning );
            return;
        }

        m_LastSendErrorLogTime = Time.realtimeSinceStartup;

        StringBuilder message = new StringBuilder();
        message.AppendLine( $"Username: {PlayerLocal.Instance.Client.NickName}" );
        message.AppendLine( $"Appversion: {VersionData.FullAppVersion}" );
        message.AppendLine( "------- Logs -------" );

        for( int i = 0; i < m_StoreForSendBuffer.Length; ++i )
        {
            if( string.IsNullOrEmpty( m_StoreForSendBuffer[ i ] ) )
            {
                continue;
            }

            int index = ( m_StoreBufferIndex - i ) % m_StoreForSendBuffer.Length;

            if( index < 0 )
            {
                index = m_StoreForSendBuffer.Length + index;
            }

            message.AppendLine( m_StoreForSendBuffer[ index ] );
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add( "GameId", PlayerLocal.Instance.Client.CurrentRoom.Name );
        parameters.Add( "Nickname", PlayerLocal.Instance.Client.NickName );
        parameters.Add( "Message", message.ToString() );

        PlayerLocal.Instance.Client.OpWebRpc( "log", parameters );

        MessagePopup.Show( "Message was sent! Thank you.", LogType.Log );
    }

    void HandleLog( string logString, string stackTrace, LogType type )
    {
        LogsHandled++;

        m_DisplayBuffer[ m_BufferIndex ] = System.DateTime.Now.ToString( "[HH:mm:ss] " ) + logString;
        m_StoreForSendBuffer[ m_StoreBufferIndex ] = type.ToString() + ": " + logString + "\n" + stackTrace;

        if( type == LogType.Warning && ShowWarningPopup)
        {
            m_DisplayBuffer[ m_BufferIndex ] = "<color=\"yellow\">" + m_DisplayBuffer[ m_BufferIndex ] + "</color>";

            MessagePopup.Show( logString, type );
        }

        if( type == LogType.Error || type == LogType.Exception || type == LogType.Assert )
        {
            m_DisplayBuffer[ m_BufferIndex ] = "<color=\"red\">" + m_DisplayBuffer[ m_BufferIndex ] + "</color>";

            MessagePopup.Show( logString, type );
        }

        m_BufferIndex = ( m_BufferIndex + 1 ) % m_DisplayBuffer.Length;
        m_StoreBufferIndex = ( m_StoreBufferIndex + 1 ) % m_StoreForSendBuffer.Length;

        LogChangedCallback?.Invoke();
    }
}
