using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource[] soundEffects;
    public GameObject BGMusic;
    private AudioSource bGAudioSource;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        bGAudioSource = BGMusic.GetComponent<AudioSource>();
    }

    public void PlaySFX(int sfxNumber)
    {
        soundEffects[sfxNumber].Stop();
        soundEffects[sfxNumber].Play();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void IncreasSound() {
        PlaySFX(0);
        for (int i = 0; i < soundEffects.Length; i++)
        {
            
            if (soundEffects[i].volume < 1)
                soundEffects[i].volume += 0.1f;
        }
    }
    public void DecreasSound() {
        PlaySFX(1);
        for (int i = 0; i < soundEffects.Length; i++)
        {
            if(soundEffects[i].volume > 0)
            soundEffects[i].volume -= 0.1f;
        }
    }
    public void IncreasMusic() {
        if (bGAudioSource.volume < 1)
            bGAudioSource.volume += 0.1f;
    }
    public void DecreasMusic() {
        if (bGAudioSource.volume > 0)
            bGAudioSource.volume -= 0.1f;
    }
}
