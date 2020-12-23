using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Groups
{

    public static class Helper
    {
        public static void DestroyAllChildren( Transform parent )
        {
            for( int i = 0; i < parent.childCount; ++i )
            {
                var child = parent.GetChild( i );
                GameObject.Destroy( child.gameObject );
            }
        }

        public static string GetAuthName( string id, string nickname )
        {
            return id + "#" + nickname;
        }

        public static string GetAuthName( Photon.Realtime.Player player )
        {
            return GetAuthName( player.UserId, player.NickName );
        }

        public static string GetIdFromSenderId( string senderId )
        {
            int idEnd = senderId.IndexOf( '#' );
            return senderId.Substring( 0, idEnd );
        }
        public static string GetNicknameFromSenderid( string senderId )
        {
            int idEnd = senderId.IndexOf( '#' );
            return senderId.Substring( idEnd + 1 );
        }
    }
}