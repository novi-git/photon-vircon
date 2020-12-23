using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Space Space;
    public Vector3 RotationSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate( RotationSpeed * Time.deltaTime, Space );
    }
}
