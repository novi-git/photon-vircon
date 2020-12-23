using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SpaceHub.Conference
{


    public class BoothDisplay : MonoBehaviour
    {
        public static UnityAction<BoothDisplay> BoothDisplayClickedCallback;
        public static BoothDisplay CurrentDisplay;
        public static bool IsInDisplay()
        {
            return CurrentDisplay != null;
        }

        public string ImageUri;
        public Renderer DisplayRenderer;
        public GameObject ViewCamera;
        public Transform AvatarTarget;
        public Transform AvoidanceTarget;
        public float AvoidanceRadius;

        public ConferenceInteractable Interactable;
        public ConferenceInteractable InteractableBack;

        Material m_Material;

        protected virtual void Start()
        {
            if( Interactable != null )
            {
                Interactable.SelectExitCallback?.AddListener( OnEnter );
                 Interactable.gameObject.SetActive( true );
            }

            if( InteractableBack != null )
            {
                InteractableBack.SelectExitCallback?.AddListener( OnExit );
                InteractableBack.gameObject.SetActive( true );
            }

            if( DisplayRenderer == null )
            {
                Debug.LogWarning( "No renderer assigned to BoothDisplay. ", gameObject );
                return;
            }
            
            if( !string.IsNullOrEmpty( ImageUri ) )
            {
               // DownloadTexture( ImageUri );
                return;
            }
            
        }


        public void SetImageUrl( string imageUri2 )
        {
            ImageUri = imageUri2;

            DownloadTexture( ImageUri );
        }

        protected virtual void DownloadTexture( string uri )
        {
            Debug.Log( "Download Texture " + uri );
            StartCoroutine( Helpers.DownloadTextureRoutine( uri, OnDownloadedTexture ) );
        }

        protected virtual void OnDownloadedTexture( Texture2D tex )
        { 
            Debug.Log("Set Material Texture ");
            m_Material = new Material( DisplayRenderer.sharedMaterial );
            m_Material.SetTexture( "_BaseMap", tex );
            DisplayRenderer.sharedMaterial = m_Material;

        }

        public void OnEnter()
        {
            AvatarMovement.SetAdditionalAvoidance( AvoidanceTarget.position, AvoidanceRadius );

            if( ViewModeManager.Instance.CurrentViewMode.GetViewMode() == ViewModeManager.ViewMode.XR )
            {
                PlayerLocal.Instance.GoToAndLook( AvatarTarget, true, true );
            }
            else
            {
                PlayerLocal.Instance.GoToAndLook( AvatarTarget, false, true );
                if( CurrentDisplay != null )
                {
                    CurrentDisplay.OnExit();
                }
                ViewCamera.SetActive( true );
                CurrentDisplay = this;

                ConferenceSceneSettings.Instance?.DoDisableTeleport();
                Interactable?.gameObject.SetActive( false );
                InteractableBack?.gameObject.SetActive( true );

                BackUI.Instance?.AddBackData( OnExit );
            }

            BoothDisplayClickedCallback?.Invoke( this );
        }

        public void OnExit()
        {
            ViewCamera.SetActive( false );
            if( CurrentDisplay == this )
            {
                CurrentDisplay = null;
            }
            InteractableBack?.gameObject.SetActive( false );
            Interactable?.gameObject.SetActive( true );

            ConferenceSceneSettings.Instance?.DoEnableTeleport();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if( enabled == false )
            {
                return;
            }

            if( AvoidanceTarget != null )
            {
                Gizmos.DrawWireSphere( AvoidanceTarget.position, AvoidanceRadius );
            }
            /*
            if( ViewCamera != null )
            {
                Gizmos.matrix = Matrix4x4.TRS( ViewCamera.transform.position, ViewCamera.transform.rotation, Vector3.one );
                Gizmos.DrawFrustum( Vector3.zero, ViewCamera.fieldOfView, ViewCamera.farClipPlane, ViewCamera.nearClipPlane, ViewCamera.aspect );
                Gizmos.matrix = Matrix4x4.identity;
            }*/

            if( AvatarTarget != null )
            {
                Gizmos.DrawSphere( AvatarTarget.position + Vector3.up * 1.575f + AvatarTarget.forward * -0.125f, 0.15f );
                Gizmos.DrawSphere( AvatarTarget.position + Vector3.up * 1.25f + AvatarTarget.forward * -0.1f, 0.25f );
            }
        }

    }
}