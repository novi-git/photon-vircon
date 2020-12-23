using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRandomizer : MonoBehaviour
{
    public AudioClip[] audioClips;
    [SerializeField] private AudioSource audioSource;

    //Vector2 pitchRange;

    void OnEnable()
    {
        SelectRandomClip();
        audioSource.PlayOneShot(audioSource.clip); 
    }

    private void SelectRandomClip()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        //audioSource.pitch = Random.Range(.95f, 1.05f);
    }
}
