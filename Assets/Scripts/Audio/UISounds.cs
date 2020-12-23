using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UISounds : MonoBehaviour
{
    public static UISounds Instance;
    public Sound[] sounds;

    private void Awake()
    {
        if( Instance != null )
        {
            DestroyImmediate( gameObject );
            return;
        }

        Instance = this;
        DontDestroyOnLoad( gameObject );

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
            s.source.outputAudioMixerGroup = s.outputAudioMixerGroup;
        }
    }

    private void PlayOneShot(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.PlayOneShot(s.clip, s.volume);
    }

    public static void Play( string name )
    {
        if( Instance == null )
        {
            return;
        }

        Instance.PlayOneShot( name );
    }

    public static void PlayConfirm()
    {
        if( Instance == null )
        {
            return;
        }

        Instance.PlayOneShot("Confirm");
    }

    public static void PlayCancel()
    {
        if( Instance == null )
        {
            return;
        }

        Instance.PlayOneShot("Cancel");
    }

    public static void PlayEmote()
    {
        if( Instance == null )
        {
            return;
        }

        Instance.PlayOneShot("Emote");
    }

}