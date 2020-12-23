using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceHub.Conference
{
    [CustomEditor( typeof( ConferenceInteractable ) )]
    public class ConferenceInteractableEditor : Editor
    {
        ConferenceInteractable m_Target;
        List<string> m_SkipProperties;

        private void OnEnable()
        {
            m_Target = (ConferenceInteractable)target;
            m_SkipProperties = new List<string>();
            m_SkipProperties.Add( "m_InteractionManager" );
            m_SkipProperties.Add( "m_InteractionLayerMask" );
            //m_SkipProperties.Add( "m_Colliders" );
            m_SkipProperties.Add( "m_InteractionLayerMask" );
            m_SkipProperties.Add( "m_OnFirstHoverEnter" );
            m_SkipProperties.Add( "m_OnHoverEnter" );
            m_SkipProperties.Add( "m_OnHoverExit" );
            m_SkipProperties.Add( "m_OnLastHoverExit" );
            m_SkipProperties.Add( "m_OnSelectEnter" );
            m_SkipProperties.Add( "m_OnSelectExit" );
            m_SkipProperties.Add( "m_OnActivate" );
            m_SkipProperties.Add( "m_OnDeactivate" );
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while( iterator.NextVisible( enterChildren ) )
            {
                if( m_SkipProperties.Contains( iterator.propertyPath ) )
                {
                    continue;
                }

                using( new EditorGUI.DisabledScope( "m_Script" == iterator.propertyPath ) )
                {
                    EditorGUILayout.PropertyField( iterator, true, new GUILayoutOption[ 0 ] );
                }
                enterChildren = false;
            }
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();

        }
    }
}
 