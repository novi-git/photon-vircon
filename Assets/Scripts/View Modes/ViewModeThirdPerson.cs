using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class ViewModeThirdPerson : ViewModeBase
    {
        public override ViewModeManager.ViewMode GetViewMode()
        {
            return ViewModeManager.ViewMode.ThirdPerson;
        }
    }
}