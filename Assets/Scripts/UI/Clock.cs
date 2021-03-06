﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class Clock : MonoBehaviour
    {
        TextMeshProUGUI m_Text;
        System.DateTime m_CurrentTime;

        private void Awake()
        {
            m_Text = GetComponent<TextMeshProUGUI>();

        }

        private void Start()
        {
            StartCoroutine( UpdateClockRoutine() );
        }

        IEnumerator UpdateClockRoutine()
        {
            var wait = new WaitForSeconds( 15f );

            while( true )
            {
                var currentTime = System.DateTime.Now;

                if( currentTime.Minute != m_CurrentTime.Minute )
                {
                    m_CurrentTime = currentTime;
                    m_Text.text = currentTime.ToString( "HH:mm" );
                }

                yield return wait;
            }
        }
    }
}