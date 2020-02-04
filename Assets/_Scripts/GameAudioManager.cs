using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameAudioManager : SingletonBehaviour<GameAudioManager>
{
    [SerializeField] private AudioSource m_MainSource = null;
    [SerializeField] private ObjectPool<AudioSource> m_SecondarySources;
    [SerializeField] private float m_MasterVolume = 0f, m_MusicVolume= 0f, m_EffectsVolume= 0f;
    [SerializeField] private AudioEvent[] m_AudioEvents = new AudioEvent[0];
    [SerializeField] private AudioClip[] m_Musics= new AudioClip[0];

    private void Start()
    {
        StartMusic();
    }

    /// Juste deux trois lignes pour faire poper l'editeur de commits
    /// <summary>
    /// Play a clip in loop
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySound(AudioClip clip, Vector3 position)
    {
        if(clip == null)
            return;
        var m_SecondarySource = m_SecondarySources.Rent();
        if(m_SecondarySource.clip != clip)
            m_SecondarySource.clip = clip;
        m_SecondarySource.Play();
    }
    
    /// <summary>
    /// Play a clip only once
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySoundOneShot(AudioClip clip, Vector3 position)
    {
        if(clip == null)
            return;
        var m_SecondarySource = m_SecondarySources.Rent();
        m_SecondarySource.PlayOneShot(clip);
    }

    /// <summary>
    /// Play corresponding sound in loop for given audio event type
    /// </summary>
    /// <param name="type"></param>
    public void PlaySound(AudioEventType type, Vector3 position)
    {
        var sound = m_AudioEvents.FirstOrDefault(x => x.type == type);
        if(sound == null)
            return;
        PlaySound(sound.clip,position);
        
    }

    /// <summary>
    /// Play corresponding sound once for given audio event type
    /// </summary>
    /// <param name="type"></param>
    public void PlaySoundOneShot(AudioEventType type, Vector3 position)
    {
        var sound = m_AudioEvents.FirstOrDefault(x => x.type == type);
        if(sound == null)
            return;
        PlaySoundOneShot(sound.clip,position);
    }

    /// <summary>
    /// Give the Music Source a Higher Pitch value
    /// </summary>
    public void FasterGameMusic()
    {
        m_MainSource.pitch += .25f;
    }

    /// <summary>
    /// Start the music loop
    /// </summary>
    public void StartMusic()
    {
        StartCoroutine(LoadNextSong());
    }

    /// <summary>
    /// Load a random song from the music pool
    /// </summary>
    private IEnumerator LoadNextSong()
    {
        var clip = m_Musics[Random.Range(0, m_Musics.Length)];
        m_MainSource.clip = clip;
        m_MainSource.Play();
        yield return new WaitForSeconds(clip.length);
        StartCoroutine(LoadNextSong());
    }
    
    /// <summary>
    /// Play the given clip as a music
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    private IEnumerator LoadNextSong(AudioClip clip)
    {
        m_MainSource.clip = clip;
        m_MainSource.Play();
        yield return new WaitForSeconds(clip.length);
        StartCoroutine(LoadNextSong());
    }
}

[Serializable]
public class AudioEvent
{
    public AudioEventType type;
    public AudioClip clip;
}

[Serializable]
public enum AudioEventType
{
    ValidateMenu,
    Throw,
    Jump,
    Hug,
    Fall,
    Die,
    Grab
}