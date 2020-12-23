using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SpaceHub.Conference
{

    public class ModUserListPages : MonoBehaviour
    {
        public TextMeshProUGUI PageText;
        public ModUserList UserList;

        public Button[] PrevObjects;
        public Button[] NextObjects;

        int m_CurrentPage;

        public void SetPage( int index )
        {
            m_CurrentPage = index;
            PageText.text = ( index + 1 ).ToString() + " / " + UserList.PageCount;
            SetButtonsInteractable( PrevObjects, index > 0 );
            SetButtonsInteractable( NextObjects, index < UserList.PageCount - 1 );
        }

        void SetButtonsInteractable( Button[] buttons, bool value )
        {
            foreach( var button in buttons )
            {
                button.interactable = value;
            }
        }

        public void TurnPage( int delta )
        {
            m_CurrentPage += delta;
            m_CurrentPage = Mathf.Max( 0, Mathf.Min( UserList.PageCount - 1, m_CurrentPage ) );

            UserList.ShowPage( m_CurrentPage );
        }


    }
}
