using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class FPSDisplay : MonoBehaviour
    {
        TextMeshProUGUI m_Text;
         
        // Start is called before the first frame update
        void Awake()
        {
            m_Text = GetComponent<TextMeshProUGUI>();

            StartCoroutine( UpdateTextRoutine() );
        }

        // Update is called once per frame
        IEnumerator UpdateTextRoutine()
        {
            while( true )
            {
                float current = (int)( 1f / Time.unscaledDeltaTime );

                m_Text.text = current.ToString() + " FPS";

                yield return new WaitForSeconds( 2f );
            }
        }
    }
}