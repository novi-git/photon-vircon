using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;


    [Range(0f, 1f)]
    public float volume;
    [Range(.5f, 1.5f)]
    public float pitch;

    public bool loop;

    [Range(0f, 1f)]
    public float spatialBlend;

    public AudioMixerGroup outputAudioMixerGroup;


    [HideInInspector]
    public AudioSource source;



}
