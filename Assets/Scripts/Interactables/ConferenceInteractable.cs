using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceHub.Conference
{
    [System.Serializable]
    public class ConferenceInteraction : UnityEvent { }

    public class ConferenceInteractable : XRBaseInteractable
    {
        public static ConferenceInteractable Selected;

        public bool IsInteractable { get; private set; }

        public ConferenceInteraction HoverEnterCallback;
        public ConferenceInteraction HoverExitCallback;
        public ConferenceInteraction SelectEnterCallback;
        public ConferenceInteraction SelectExitCallback;


        public UnityAction<bool> IsInteractableChangedCallback;

        XRBaseInteractor m_LastSelectInteractor;

        new void Awake()
        {
            if( ViewModeManager.SelectedViewMode == ViewModeManager.ViewMode.XR )
            {
                base.Awake();
            }
        }

        private void Start()
        {
            onFirstHoverEnter.AddListener( OnFirstHoverEnter );
            onLastHoverExit.AddListener( OnLastHoverExit );
        }

        public void TriggerHoverEnter()
        {
            HoverEnterCallback?.Invoke();
        }

        public void TriggerHoverExit()
        {
            HoverExitCallback?.Invoke();
        }

        public void TriggerSelectEnter()
        {
            Selected = this;

            SelectEnterCallback?.Invoke();
        }

        public void TriggerSelectExit()
        {
            SelectExitCallback?.Invoke();

            if( Selected == this )
            {
                Selected = null;
            }
        }
        
        protected void OnFirstHoverEnter( XRBaseInteractor interactor )
        {
            TriggerHoverEnter();
        }

        protected void OnLastHoverExit( XRBaseInteractor interactor )
        {
            TriggerHoverExit();
        }

        protected override void OnSelectEnter( XRBaseInteractor interactor )
        {
            m_LastSelectInteractor = interactor;

            TriggerSelectEnter();

            base.OnSelectEnter( interactor );
        }

        protected override void OnSelectExit( XRBaseInteractor interactor )
        {
            TriggerSelectExit();

            base.OnSelectExit( interactor );
        }
        
        public XRBaseInteractor GetLastSelectInteractor()
        {
            return m_LastSelectInteractor;
        }

        public virtual void SetIsInteractable( bool value )
        {
            IsInteractable = value;

            IsInteractableChangedCallback?.Invoke( value );
        }
    }
}
