using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpaceHub.Conference
{
    [CustomEditor( typeof( ViewModeManager ) )]
    public class ViewModeManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ViewModeManager manager = (ViewModeManager)target;

            DrawDefaultInspector();

            ViewModeManager.ViewMode newMode = ( ViewModeManager.ViewMode)EditorGUILayout.EnumPopup( "Selected View Mode", manager.GetSelectedViewMode() );

            if( newMode != manager.GetSelectedViewMode() )
            {
                manager.SetSelectedViewMode( newMode );
            }

            if( manager.HasViewMode( newMode ) == false )
            {
                EditorGUILayout.HelpBox( "The view mode '" + newMode.ToString() + "' does not exist in the ViewModes array!", MessageType.Error );
            }
        }
    }
}