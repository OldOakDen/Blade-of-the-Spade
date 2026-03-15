using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource, detectSoundsSource, loopSfxSource, pinpointSoundsSource, loopAmbientSource;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if(s == null)
        {
            Debug.Log("Hudba "+name+" nenalezena!");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Zvuk " + name + " nenalezen!");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void PlayDetectSound(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Zvuk " + name + " nenalezen!");
        }
        else
        {
            detectSoundsSource.PlayOneShot(s.clip);
        }
    }
    public void PlayPinpointSound(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Zvuk " + name + " nenalezen!");
        }
        else
        {
            pinpointSoundsSource.clip = s.clip;
            pinpointSoundsSource.Play();
        }
    }

    public void PlayLoopSound(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Zvuk " + name + " nenalezen!");
        }
        else
        {
            loopSfxSource.clip = s.clip;
            loopSfxSource.Play();
        }
    }

    public void PlayLoopAmbient(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Zvuk " + name + " nenalezen!");
        }
        else
        {
            loopAmbientSource.clip = s.clip;
            loopAmbientSource.Play();
        }
    }

}
