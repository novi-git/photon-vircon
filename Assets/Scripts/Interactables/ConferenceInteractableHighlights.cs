using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    [RequireComponent( typeof( ConferenceInteractable ))]
    public class ConferenceInteractableHighlights : MonoBehaviour
    {
        ConferenceInteractable m_Interactable;

        public Renderer[] Renderers;
        public Material[] HighlightMaterials;
        public Material[] InvalidMaterials;

        public Renderer[] HighlightEnable;
        public Renderer[] HighlightDisable;

        public Renderer[] SelectedEnable;

        public bool UnhighlightDuringSelection = true;

        Material[] m_DefaultMaterials;
        bool m_IsSelected;

        private void Awake()
        {
            m_Interactable = GetComponent<ConferenceInteractable>();
        }

        private void Start()
        {
            StoreDefaultMaterials();

            SetActive( HighlightEnable, false );
            SetActive( HighlightDisable, true );
            SetActive( SelectedEnable, false );


            m_Interactable.HoverEnterCallback.AddListener( OnHoverEnter );
            m_Interactable.HoverExitCallback.AddListener( OnHoverExit );

            if( UnhighlightDuringSelection == true )
            {
                m_Interactable.SelectEnterCallback.AddListener( OnSelectEnter );
                m_Interactable.SelectExitCallback.AddListener( OnSelectExit );
            }
        }

        private void OnDisable()
        {
            OnHoverExit();
        }

        void OnSelectEnter()
        {
            OnHoverExit();

            m_IsSelected = true;

            SetActive( SelectedEnable, true );
        }

        void OnSelectExit()
        {
            m_IsSelected = false;
            SetActive( SelectedEnable, false );
        }

        void OnHoverEnter()
        {
            if( m_IsSelected == true )
            {
                return;
            }

            SetActive( HighlightEnable, true );
            SetActive( HighlightDisable, false );
            ApplyMaterials( HighlightMaterials );
        }

        void OnHoverExit()
        {
            if( m_IsSelected == true )
            {
                return;
            }

            SetActive( HighlightEnable, false );
            SetActive( HighlightDisable, true );
            ApplyMaterials( m_DefaultMaterials );
        }

        void SetActive( Renderer[] list, bool value )
        {
            if( list == null )
            {
                return;
            }

            foreach( var rend in list )
            {
                rend.enabled = value;
            }
        }

        void StoreDefaultMaterials()
        {
            m_DefaultMaterials = new Material[ Renderers.Length ];
            for( int i = 0; i < Renderers.Length; ++i )
            {
                m_DefaultMaterials[ i ] = Renderers[ i ].material;
            }
        }

        void ApplyMaterials( Material[] newMaterials )
        {
            if ( Renderers == null || Renderers.Length == 0)
            {
                return;
            }

            if( newMaterials.Length != Renderers.Length )
            {
                Debug.LogError( "Not enough materials provided for all renderers!" );
                return;
            }

            for( int i = 0; i < Renderers.Length; ++i )
            {
                Renderers[ i ].material = newMaterials[ i ];
            }
        }
    }
}