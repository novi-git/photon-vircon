using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgm : MonoBehaviour
{
    public static bgm Instance;
    public AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(transform.parent.gameObject);
    }

    void Start()
    {
        gameObject.GetComponent<AudioSource>();
        audioSource.Play();
        
    }

    public void stopBgm()
    {
        StartCoroutine(FadeAudioSource.StartFade(audioSource, 6f, 0f));
    }


}
