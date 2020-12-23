using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class TeleportReticlePC : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Destroy( gameObject, 1f );
        }
    }
}