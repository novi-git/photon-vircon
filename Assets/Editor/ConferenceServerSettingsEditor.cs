using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpaceHub.Conference
{
    [CustomEditor( typeof( ConferenceServerSettings ))]
    public class ConferenceServerSettingsEditor : Editor
    {
        ConferenceServerSettings m_Target;

        public override void OnInspectorGUI()
        {
            m_Target = (ConferenceServerSettings)target;

            base.OnInspectorGUI();

            DrawSettingProperty( "RealtimeAppId" );
            DrawSettingProperty( "VoiceAppId" );
            DrawSettingProperty( "ChatAppId" );
            DrawSettingProperty( "ApiBaseUrl" );
            EditorGUILayout.HelpBox( "ApiBaseUrl is only needed if you want to make use of the Multi-Event functionality and you have implemented the web source on your own server. To test multi event functionality you can input https://spacehub.world/api", MessageType.Info );
        }

        void DrawSettingProperty( string propertyName )
        {
            var property = serializedObject.FindProperty( propertyName );
            string editorProperty = PlayerPrefs.GetString( propertyName );

            if( string.IsNullOrEmpty( editorProperty ) )
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField( property );
                    if( GUILayout.Button( "Save", GUILayout.Width( 50 ) ) )
                    {
                        PlayerPrefs.SetString( propertyName, property.stringValue );

                        if( m_Target.StoreAppIdsInVersionControl == false )
                        {
                            property.stringValue = "";
                        }                        
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField( propertyName, editorProperty );

                    if( GUILayout.Button( "Clear", GUILayout.Width( 50 ) ) )
                    {
                        PlayerPrefs.SetString( propertyName, "" );
                    }
                }
               
                GUILayout.EndHorizontal();

                if( m_Target.StoreAppIdsInVersionControl == true && string.IsNullOrEmpty( property.stringValue ) )
                {
                    property.stringValue = PlayerPrefs.GetString( propertyName );
                }

                if( m_Target.StoreAppIdsInVersionControl == false && !string.IsNullOrEmpty( property.stringValue ) )
                {
                    property.stringValue = "";
                }
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
