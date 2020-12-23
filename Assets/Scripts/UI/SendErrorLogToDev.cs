using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SendErrorLogToDev : MonoBehaviour
    {
        public void OnClick()
        {
            LogManager.Instance.SendLogToDeveloper();
        }
    }
}

