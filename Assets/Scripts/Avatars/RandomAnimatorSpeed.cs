using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorSpeed : MonoBehaviour
{
    public float MinSpeed= 0.7f;
    public float MaxSpeed = 1.2f;
    void Start()
    {
        var anim = GetComponent<Animator>();
        anim.speed = Random.Range( MinSpeed, MaxSpeed );
    }

}
