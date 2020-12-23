using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor( typeof( StageAttendeePlacer ) )]
public class StageAttendeePlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = ( target as StageAttendeePlacer );

        if( GUILayout.Button( "Place" ) )
        {
            t.Place();
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}
