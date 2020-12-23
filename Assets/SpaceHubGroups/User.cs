using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Groups
{
    public class User
    {
        public User( string senderId )
        {
            SenderId = senderId;
            UserId = Helper.GetIdFromSenderId( senderId );
            Nickname = Helper.GetNicknameFromSenderid( senderId );
        }

        public string SenderId { get; set; }
        public string UserId { get; set; }
        public string Nickname { get; set; }
        //public string IconUrl { get; set; }

    }
}
