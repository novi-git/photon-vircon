
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class ConnectingOverlay : MonoBehaviour
    {
        public static ConnectingOverlay Instance;

        public TextMeshProUGUI DebugText;

        Canvas m_Canvas;
        float m_LastForceShowTime;
        float m_LastStatusChangeTime;
        ClientState m_CurrentState;

        public static bool IsOpen
        {
            get
            {
                if( Instance == null || Instance.m_Canvas == null )
                {
                    return false;
                }

                return Instance.m_Canvas.enabled;
            }
        }

        public static void Show()
        {
            if( Instance != null )
            {
                Instance.DebugText.text = "";
                Instance.m_LastForceShowTime = Time.realtimeSinceStartup;
                Instance.m_Canvas.enabled = true;
            }
        }

        private void Awake()
        {
            Instance = this;

            m_Canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            PlayerLocal.Instance.Client.StateChanged += OnStateChanged;
            DebugText.text = PlayerLocal.Instance.Client.State.ToString();
        }

        private void OnDisable()
        {
            if( PlayerLocal.Instance != null &&
                PlayerLocal.Instance.Client != null )
            {
                PlayerLocal.Instance.Client.StateChanged += OnStateChanged;
            }
        }

        void OnStateChanged( ClientState previousState, ClientState newState )
        {
            m_CurrentState = newState;
            DebugText.text = m_CurrentState.ToString();
            m_LastStatusChangeTime = Time.realtimeSinceStartup;
        }

        void FixedUpdate()
        {
            bool showConnectingOverlay = !PlayerLocal.Instance.Client.IsConnectedAndReady 
                || !PlayerLocal.Instance.Client.InRoom 
                || ( Time.realtimeSinceStartup - m_LastForceShowTime ) < 1f;


            m_Canvas.enabled = showConnectingOverlay;

            if( showConnectingOverlay == true )
            {
                float timeSinceLastStatusUpdate = Time.realtimeSinceStartup - m_LastStatusChangeTime;

                if( timeSinceLastStatusUpdate > 3f )
                {
                    float retryIn = 8f - timeSinceLastStatusUpdate;

                    DebugText.text = $"{m_CurrentState.ToString()}\nRetry in {Mathf.CeilToInt(retryIn)} seconds...";

                    if( retryIn <= 0f )
                    {
                        PlayerLocal.Instance.Connector.Reconnect();
                    }
                }
            }
            else
            {
                m_LastStatusChangeTime = Time.realtimeSinceStartup;
            }
        }
    }
}