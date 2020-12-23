using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class StageAttendeesCount : MonoBehaviour
    {
        public StageHandlerBase StageHandler;
        public TextMeshProUGUI Text;
        public float UpdateInterval = 1f;


        float m_NextUpdateTime = 0;

        private void Update()
        {
            
            if ( Time.realtimeSinceStartup > m_NextUpdateTime )
            {
                Text.text = StageHandler.GetAttendeesCount().ToString();
                m_NextUpdateTime = Time.realtimeSinceStartup + UpdateInterval;
            }
        }
    }
}
