using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{


    public class TabButton : MonoBehaviour
    {
        List<TabButton> m_SiblingTabButtons = new List<TabButton>();

        public bool DefaultEnabled;
        public GameObject Tab;
        public Image Image;


        private void Awake()
        {
            m_SiblingTabButtons = new List<TabButton>( transform.parent.GetComponentsInChildren<TabButton>() );
            m_SiblingTabButtons.Remove( this );
        }

        private void Start()
        {
            if( DefaultEnabled )
            {
                OnClick();
            }
        }

        public void OnClick()
        {
            SetSelected( true );
            HideOthers();
        }

        void SetSelected( bool value )
        {
            Tab.SetActive( value );
            Image.color = new Color( 1f, 1f, 1f, value ? 1f : 0.5f );
        }

        void HideOthers()
        {
            foreach( var button in m_SiblingTabButtons )
            {
                button.HidePanel();
            }
        }

        public void HidePanel()
        {
            SetSelected( false );
        }
    }
}