using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace SpaceHub.Groups
{

    public class ChannelPermissions
    {
        public ChannelPermissions( GroupsManager groupsManager )
        {
            m_Manager = groupsManager;
            LoadFromPlayerPrefs();
        }

        GroupsManager m_Manager;

        public UnityAction BlockedChannelsChangedCallback;

        public HashSet<string> ConfirmedPrivateChats { get; private set; } = new HashSet<string>();

        [System.Serializable]
        class PermissionData
        {
            public string[] BlockedUsers;
            public string[] BlockedChannels;
            public string[] MutedUsers;
        }

        public HashSet<string> BlockedUsers { get; private set; } = new HashSet<string>();
        public HashSet<string> BlockedChannels { get; private set; } = new HashSet<string>();
        public HashSet<string> MutedUsers { get; private set; } = new HashSet<string>();


        public void ConfirmPrivateChat( string channelName )
        {
            if( ConfirmedPrivateChats.Contains( channelName ) )
            {
                return;
            }
            ConfirmedPrivateChats.Add( channelName );

            SaveToPlayerPrefs();
        }

        public bool IsPrivateChatConfirmed( string channelName )
        {
            return ConfirmedPrivateChats.Contains( channelName );
        }

        public bool IsUserBlocked( string senderId )
        {
            return BlockedUsers.Contains( senderId );
        }

        public bool IsChannelBlocked( string channel )
        {
            return BlockedChannels.Contains( channel );
        }

        public bool IsUserMuted( string senderId )
        {
            return MutedUsers.Contains( senderId ) || BlockedUsers.Contains( senderId );
        }

        public void MuteUser( string senderId )
        {
            if( MutedUsers.Contains( senderId ) )
            {
                return;
            }
            MutedUsers.Add( senderId );

            SaveToPlayerPrefs();
            OnMutedUsersChanged();
        }

        public void UnmuteUser( string senderId )
        {
            if( MutedUsers.Contains( senderId ) )
            {
                MutedUsers.Remove( senderId );
            }

            SaveToPlayerPrefs();
            OnMutedUsersChanged();
        }


        void OnMutedUsersChanged()
        {
            VoiceMute.OnMuteStatusChanged();
        }

        public void BlockUser( string senderId )
        {
            if( BlockedUsers.Contains( senderId ) )
            {
                return;
            }

            BlockedUsers.Add( senderId );
            BlockedChannels.Add( m_Manager.Client.GetPrivateChannelNameByUser( senderId ) );

            BlockedChannelsChangedCallback?.Invoke();

            SaveToPlayerPrefs();
        }



        public void UnblockChannel( string channel )
        {
            if( BlockedChannels.Contains( channel ) )
            {
                BlockedChannels.Remove( channel );
            }

            SaveToPlayerPrefs();
        }
        public void BlockChannel( string channel )
        {
            if( BlockedChannels.Contains( channel ) == false )
            {
                BlockedChannels.Add( channel );
            }

            SaveToPlayerPrefs();
        }

        public void UnblockUser( string senderId )
        {
            if( BlockedUsers.Contains( senderId ) )
            {
                BlockedUsers.Remove( senderId );
                var channelName = m_Manager.Client.GetPrivateChannelNameByUser( senderId );
                if( BlockedChannels.Contains( channelName ) )
                {
                    BlockedChannels.Remove( channelName );
                }
                BlockedChannelsChangedCallback?.Invoke();
            }
            SaveToPlayerPrefs();
        }

        void LoadFromPlayerPrefs()
        {
            try
            {
                var data = JsonUtility.FromJson<PermissionData>( PlayerPrefs.GetString( "Permissions" ) );
                if( data != null )
                {
                    if( data.MutedUsers != null ) MutedUsers = new HashSet<string>( data.MutedUsers );
                    if( data.BlockedChannels != null ) BlockedChannels = new HashSet<string>( data.BlockedChannels );
                    if( data.BlockedUsers != null ) BlockedUsers = new HashSet<string>( data.BlockedUsers );
                }

            }
            catch( System.Exception e )
            {
                Debug.LogException( e );
            }
        }

        void SaveToPlayerPrefs()
        {
            var data = new PermissionData()
            {
                MutedUsers = MutedUsers.ToArray(),
                BlockedUsers = BlockedUsers.ToArray(),
                BlockedChannels = BlockedChannels.ToArray(),
            };

            PlayerPrefs.SetString( "Permissions", JsonUtility.ToJson( data ) );
            PlayerPrefs.Save();
        }
    }
}
