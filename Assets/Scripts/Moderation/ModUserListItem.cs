using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    using FlagType = ModUserList.FlagType;


    public class ModUserListItem : MonoBehaviour
    {
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Company;
        public TextMeshProUGUI Room;
        public TextMeshProUGUI Reports;
        public TextMeshProUGUI Role;
        public TextMeshProUGUI Flags;
        public TextMeshProUGUI Version;

        ModUserList.UserListData m_Data;

        public void OpenPopup()
        {
            ModUserPopup.Instance.Show( m_Data );
        }

        public void SetData( ModUserList.UserListData data )
        {
            m_Data = data;

           
            Name.text = data.GetDisplayName();

            Company.text = data.Company;
            Room.text = data.GameId;

            Color col = data.Reports == 0 ? Color.white : data.Reports < 3 ? Color.yellow : Color.red;
            Reports.color = col;
            Reports.text = data.Reports.ToString();

            if( data.Roles != null && data.Roles.Length > 0 )
            {
                string roles = "";
                foreach( var role in data.Roles )
                {
                    roles += role.RoleType.ToString();
                    roles += "\n";
                }
                Role.text = roles;
            }
            else
            {
                Role.text = " - ";
            }

            Flags.text = GetFlagNames( data.Flags );
            Version.text = data.AppVersion;
        }

        string GetFlagNames( FlagType flags )
        {
            if( flags == 0 )
            {
                return " - ";
            }

            var sb = new System.Text.StringBuilder();
            var list = System.Enum.GetValues( typeof( FlagType ) );
            for( int i = 1; i < list.Length; ++i )
            {
                var type = (FlagType)list.GetValue( i );
                if( flags.HasFlag( type ) )
                {
                    if ( sb.Length > 0 )
                    {
                        sb.Append( ", " );
                    }
                    sb.Append( type.ToString() );
                }
            }

            return sb.ToString();
        }
    }
}
