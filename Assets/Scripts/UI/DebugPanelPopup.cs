using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class DebugPanelPopup : PopupBase
    {
        public static DebugPanelPopup Instance;

        void Start()
        {
            Instance = this;

            Close();
            //PlayerLocal.Instance.OnTeleportCallback += Close;
        }

        public void Open()
        {
            DoOpen();
        }

        public void Close()
        {
            DoClose();
        }
    }
}