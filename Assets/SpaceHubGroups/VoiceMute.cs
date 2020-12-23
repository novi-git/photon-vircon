using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Groups
{
    public class VoiceMute : MonoBehaviour
    {
        static List<VoiceMute> s_List = new List<VoiceMute>();

        public static void OnMuteStatusChanged()
        {
            foreach( var item in s_List )
            {
                item.UpdateMuteStatus();
            }
        }



        public string ChatId;
        public ChannelPermissions m_Permissions;

        AudioSource m_AudioSource;

        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            s_List.Add( this );
        }

        private void OnDisable()
        {
            s_List.Remove( this );
        }

        public void UpdateMuteStatus()
        {
            //Debug.Log( "Check mutestatus for " + ChatId + ": " + m_Permissions.IsUserMuted( ChatId ) );
            m_AudioSource.volume = m_Permissions.IsUserMuted( ChatId ) ? 0f : 1f;
        }

    }
}