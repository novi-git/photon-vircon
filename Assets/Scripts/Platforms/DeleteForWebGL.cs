using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteForWebGL : MonoBehaviour
{
    void OnEnable()
    {
#if UNITY_WEBGL
        DestroyImmediate( gameObject );
#endif
    }
}
