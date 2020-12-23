using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class DeactivateForNonAdmin : MonoBehaviour
    {
        // Start is called before the first frame update
        IEnumerator Start()
        {
            while( PlayerLocal.Instance == null || PlayerLocal.Instance.Connector.Network.Client.IsConnectedAndReady == false )
            {
                yield return null;
            }

            gameObject.SetActive( PlayerLocal.Instance.Connector.IsAdmin );
        }
    }
}