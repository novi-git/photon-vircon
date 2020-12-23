using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent( typeof( Button ) )]
public class WebGLButtonHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Button.ButtonClickedEvent Events;

    //When triggering OpenURL in the browser, this event has to be called in OnPointerDown in order for the OpenURL Plugin to be able to open the url in a new tab
    //Browser security stuff. But togther with Helpers.OpenURL() it works :)
    public void OnPointerDown( PointerEventData eventData )
    {
#if UNITY_WEBGL
        Events.Invoke();
        EventSystem.current.SetSelectedGameObject( null );
#endif
    }

    public void OnPointerUp( PointerEventData eventData )
    {
        
    }

    public void OnPointerClick( PointerEventData eventData )
    {
#if !UNITY_WEBGL
        Events.Invoke();
#endif
    }
}
