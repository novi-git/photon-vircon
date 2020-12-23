using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSnapshotManager : MonoBehaviour
{
    public AudioMixerSnapshot inside;
    public AudioMixerSnapshot outside;


    public void AudioInside()
    {
        inside.TransitionTo(2.5f);
    }

    public void AudioOutside()
    {
        outside.TransitionTo(2.5f);
    }

}
