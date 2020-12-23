using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class ViewModeManager : MonoBehaviour
    {
        public enum ViewMode
        {
            XR,
            ThirdPerson,
        }

        public static ViewModeManager Instance;

        public ViewModeBase[] ViewModes;
        
        public UnityAction<ViewModeBase> ViewModeSelectedCallback;

        public ViewModeBase CurrentViewMode { get; private set; }

        public static ViewMode SelectedViewMode
        {
            get
            {
                return (ViewMode)PlayerPrefs.GetInt( "SelectedViewMode", 1 );
            }
        }


        void OnEnable()
        {
            Instance = this;
        }

        void Awake()
        {
            LoadSelectedViewMode();
        }

        void LoadSelectedViewMode()
        {
            ViewMode selectedViewMode = GetSelectedViewMode();
            ViewMode useViewMode = selectedViewMode;

#if !UNITY_EDITOR
#if UNITY_ANDROID
            useViewMode = ViewMode.ThirdPerson;
#endif

#if UNITY_STANDALONE || UNITY_WEBGL
            useViewMode = ViewMode.ThirdPerson;
#endif

#if FORCE_XR
            useViewMode = ViewMode.XR;
#endif
#endif

            if( useViewMode != selectedViewMode )
            {
                SetSelectedViewMode( useViewMode );
            }

            if( HasViewMode( useViewMode ) == false )
            {
                Debug.LogError( "The view mode '" + useViewMode.ToString() + "' does not exist in the ViewModes array!" );
                return;
            }

            for( int i = 0; i < ViewModes.Length; ++i )
            {
                if( ViewModes[ i ].GetViewMode() == GetSelectedViewMode() )
                {
                    Debug.Log( "Selected view mode: " + GetSelectedViewMode().ToString() );
                    ViewModes[ i ].OnViewModeSelected();
                    ViewModeSelectedCallback?.Invoke( ViewModes[ i ] );
                    CurrentViewMode = ViewModes[ i ];
                }
                else
                {
                    ViewModes[ i ].OnViewModeDeselected();
                }
            }
        }

        private void Update()
        {
            if ( PlayerLocal.IsPlayerTyping() )
            {
                return;
            }
            
            if( Input.GetKey( KeyCode.LeftShift ) )
            {
                for( int i = 0; i < ViewModes.Length; ++i )
                {
                    if( Input.GetKeyDown( KeyCode.Alpha1 + i ) )
                    {
                        SetSelectedViewMode( (ViewMode)i );
                        LoadSelectedViewMode();
                    }
                }
            }
        }

        
        public ViewMode GetSelectedViewMode()
        {
            return ViewModeManager.SelectedViewMode;
        }

        public void SetSelectedViewMode( ViewMode newMode )
        {
            PlayerPrefs.SetInt( "SelectedViewMode", (int)newMode );
        }

        public bool HasViewMode( ViewMode mode )
        {
            if( ViewModes == null )
            {
                return false;
            }

            for( int i = 0; i < ViewModes.Length; ++i )
            {
                if( ViewModes[ i ].GetViewMode() == mode )
                {
                    return true;
                }
            }

            return false;
        }
    }
}