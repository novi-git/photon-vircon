using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ambience : MonoBehaviour
{
    public AudioManager audioRef;

    void Start()
    {
        audioRef.PlayRandomPosition("Ambience1");
        audioRef.PlayRandomPosition("Ambience2");
        audioRef.PlayRandomPosition("Ambience3");
    }

}
