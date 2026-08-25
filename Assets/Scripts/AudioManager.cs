using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public Sound[] musicSounds;

    [Header("SFX")]
    public Sound[] sfxSounds;

    [Header("Audio Sources")]
    public AudioSource AudioSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        PlayMusic("Theme");
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("Music not found: " + name);
            return;
        }

        if (s.clip == null)
        {
            Debug.LogWarning("AudioClip is missing for music: " + name);
            return;
        }

        AudioSource.clip = s.clip;
        AudioSource.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("SFX not found: " + name);
            return;
        }

        if (s.clip == null)
        {
            Debug.LogWarning("AudioClip is missing for SFX: " + name);
            return;
        }

        sfxSource.PlayOneShot(s.clip);
    }

    public void ToggleMusic()
    {
        AudioSource.mute = !sfxSource.mute;
    }

    public void MusicVolume(float volume)
    {
        AudioSource.volume= volume;
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
    private void Update()
    {
        MusicVolume(MainMenuManager.Instance.bgmVolume);
        SFXVolume(MainMenuManager.Instance.sfxVolume);
    }
}