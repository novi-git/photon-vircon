using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceHub.Conference
{


    public class UiFunnelManager : MonoBehaviour
    {
        public List<UIFunnelScreen> Screens = new List<UIFunnelScreen>();
        public List<Button> HeadlineButtons = new List<Button>();

        int m_CurrentScreenIndex = 0;
        int m_MaximumScreen = 0;

        private void Start()
        {
            for( int i = 0; i < HeadlineButtons.Count; ++i )
            {
                int index = i;
                HeadlineButtons[ i ].onClick.AddListener( () =>
                  {
                      ShowScreenIndex( index );
                  } );
            }

            foreach( var screen in Screens )
            {
                screen.gameObject.SetActive( true );
            }

            ShowScreenIndex( 1 );
        }

        [ContextMenu( "Next" )]
        public void Next()
        {
            ShowScreenIndex( m_CurrentScreenIndex + 1 );
        }

        public void NextScene() {
            PlayerPrefs.SetString("EventId", "");
            SceneManager.LoadScene("CustomizationRoom");
            PlayerPrefs.SetString("PhotonRegion", "asia");
            Debug.Log("Load Customization Room");
        }

        [ContextMenu( "Previous" )]
        public void Previous()
        {
            ShowScreenIndex( m_CurrentScreenIndex - 1 );
        }

        public void ShowScreenIndex( int index )
        {
            bool forward = index >= m_CurrentScreenIndex;

            m_CurrentScreenIndex = Mathf.Clamp( index, 0, Screens.Count - 1 );
            m_MaximumScreen = Mathf.Max( m_MaximumScreen, m_CurrentScreenIndex );

            UpdateVisibility( forward );
        }

        public void FadeInCurrent()
        {
            Screens[ m_CurrentScreenIndex ].Show();
        }

        public void SetMaximumScreenToCurrent()
        {
            m_MaximumScreen = m_CurrentScreenIndex;
            UpdateVisibility();
        }

        void UpdateVisibility( bool forward = true )
        {
            for( int i = 0; i < Screens.Count; ++i )
            {
                Screens[ i ].SetVisible( i == m_CurrentScreenIndex, forward );
            }

            for( int i = 0; i < HeadlineButtons.Count; ++i )
            {
                HeadlineButtons[ i ].interactable = i <= m_MaximumScreen;
            }
        }



    }
}
