using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class MainMenuPopup : PopupBase
    {
        public static MainMenuPopup Instance;

        void Start()
        {
            Instance = this;

            Open();
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

        public void CloseAndPopupDebug()
        {
            DoClose();
            DebugPanelPopup.Instance.Open();
        }

        public void QuitApplication()
        {
            Application.Quit();
        }
    }
}