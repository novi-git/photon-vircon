using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStartRandomPosition : MonoBehaviour
{
    public AudioClip audioClip;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        int randomStartTime = Random.Range(0, audioClip.samples - 1);
        audioSource.timeSamples = randomStartTime;
        audioSource.Play();
    }

}
