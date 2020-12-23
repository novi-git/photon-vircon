using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class ConferenceSceneSettings : MonoBehaviour
    {
        public static ConferenceSceneSettings Instance;

        public bool EnableTeleport = true;
        public bool EnableLook = true;
        public bool EnableRemoteAvatars = true;
        public bool EnableCameraToggle = true;
        public float AreaWidth = 32f;
        public float AreaLength = 32f;
        public float AreaHeight = 8f;
        public int HighFrequencyGroupSeparations = 3;
        public int maxPlayers = 200;
        
        public bool ChatLogEnabledForRoom = false;
        public string ChatRoomChannelDisplayName = "Public Floor Channel";

        public bool ChatInputEnabled = true;
        public string ChatInputPromptActive = "Type your message here...";
        public string ChatInputPromptInactive = "Press Enter to start typing...";

        int m_TeleportDisabledOverride = 0;

        private void OnEnable()
        {
            Instance = this;
        }

        private void Start()
        {
            if( ConferenceRoomManager.Instance == null )
            {
                ConferenceRoomManager.LoadRoom( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, null, null );
            }
        }

        public bool IsTeleportEnabled()
        {
            return EnableTeleport && m_TeleportDisabledOverride <= 0;
        }

        public bool IsLookEnabled()
        {
            return EnableLook && BoothDisplay.IsInDisplay() == false;
        }

        public void DoDisableTeleport()
        {
            m_TeleportDisabledOverride++;
        }
        public void DoEnableTeleport()
        {
            m_TeleportDisabledOverride = Mathf.Max( 0, m_TeleportDisabledOverride - 1 );
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube( Vector3.zero, new Vector3( AreaWidth, AreaHeight, AreaLength ) );

            if( HighFrequencyGroupSeparations > 0 )
            {
                float groupWidth = AreaWidth / HighFrequencyGroupSeparations;
                float groupLength = AreaLength / HighFrequencyGroupSeparations;
                Vector3 groupSize = new Vector3( groupWidth, 1f, groupLength ) * 0.99f;

                

                byte playerGroupId = 255;

                if( PlayerLocal.Instance != null )
                {
                    playerGroupId = PlayerLocal.Instance.CurrentHighFrequencyInterestGroupId;


                    Handles.BeginGUI();
                    GUI.Label( new Rect( 10, 10, 100, 20 ), "PlayerGroupId: " + playerGroupId );
                    Handles.EndGUI();
                }

                for( int x = 0; x < HighFrequencyGroupSeparations; ++x )
                {
                    for( int z = 0; z < HighFrequencyGroupSeparations; ++z )
                    {
                        Vector3 center = Vector3.zero;
                        center.x = x * groupWidth - AreaWidth * 0.5f + groupWidth * 0.5f;
                        center.z = z * groupLength - AreaLength * 0.5f + groupLength * 0.5f;

                        byte groupId = Helpers.GetHighFrequencyGroupId( center );
                        Gizmos.color = groupId == playerGroupId ? Color.red : Color.cyan;
                        Gizmos.DrawWireCube( center, groupSize );
                    }
                }
            }
        }
#endif
    }
}