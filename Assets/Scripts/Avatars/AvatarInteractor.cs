using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceHub.Conference
{
    public class AvatarInteractor : MonoBehaviour
    {
        public void OnAvatarClicked()
        {
            AvatarInfoPopup.Instance.Open( GetComponent<Avatar>() );
        }
    }
}