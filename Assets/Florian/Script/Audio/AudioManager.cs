using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Instance
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                    instance = new GameObject("Spawned AudioManager", typeof(AudioManager)).GetComponent<AudioManager>();
            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    #region Fields
    private AudioSource musicSource;
    private AudioSource ambiantSource;
    private AudioSource musicSource2;
    private AudioSource sfxSource;
    private float musicVolume = 1;
    // Multiple musics
    private bool firstMusicSourceIsActive;
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource2 = gameObject.AddComponent<AudioSource>();
        ambiantSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource2.loop = true;
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null)
            musicSource.Stop();
        else
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }
        public void PlayAmbiant(AudioClip ambiant, float volume)
    {
        if (ambiant == null)
            ambiantSource.Stop();
        else
        {
            ambiantSource.clip = ambiant;
            ambiantSource.volume = volume;
            ambiantSource.Play();
        }
    }

    public void PlayMusicWithFade(AudioClip musicClip, float transitionTime = 1.0f)
    {
        AudioSource activeSource = (firstMusicSourceIsActive) ? musicSource : musicSource2;
        StartCoroutine(UpdateMusicWithFade(activeSource, musicClip, transitionTime));
    }

    public void PlayMusicWithCrossFade(AudioClip musicClip, float transitionTime = 1.0f)
    {
        AudioSource activeSource = (firstMusicSourceIsActive) ? musicSource : musicSource2;
        AudioSource newSource = (firstMusicSourceIsActive) ? musicSource2 : musicSource;

        firstMusicSourceIsActive = !firstMusicSourceIsActive;

        newSource.clip = musicClip;
        newSource.Play();
        StartCoroutine(UpdateMusicWithCrossFade(activeSource, newSource, musicClip, transitionTime));
    }

    private IEnumerator UpdateMusicWithFade(AudioSource activeSource, AudioClip music, float transitionTime)
    {
        if (!activeSource.isPlaying)
            activeSource.Play();

        float t = 0.0f;

        // Fade out
        for (t = 0.0f; t <= transitionTime; t += Time.deltaTime)
        {
            activeSource.volume = (musicVolume - ((t / transitionTime) * musicVolume));
            yield return null;
        }

        // Swap to the new music clip
        activeSource.Stop();
        activeSource.clip = music;
        activeSource.Play();

        // Fade in
        for (t = 0.0f; t <= transitionTime; t += Time.deltaTime)
        {
            activeSource.volume = (t / transitionTime) * musicVolume;
            yield return null;
        }
        activeSource.volume = musicVolume;
    }
    private IEnumerator UpdateMusicWithCrossFade(AudioSource original, AudioSource newSource, AudioClip music, float transitionTime)
    {
        if (!original.isPlaying)
            original.Play();
        newSource.Stop();
        newSource.clip = music;
        newSource.Play();

        float t = 0.0f;

        for (t = 0.0f; t <= transitionTime; t += Time.deltaTime)
        {
            original.volume = (musicVolume - ((t / transitionTime) * musicVolume));
            newSource.volume = (t / transitionTime) * musicVolume;
            yield return null;
        }

        original.volume = 0;
        newSource.volume = musicVolume;

        original.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        sfxSource.PlayOneShot(clip, volume);
    }
    public void Play3DSourceSFX(AudioClip clip, AudioSource source, float volume)
    {
        source.PlayOneShot(clip, volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = musicSource.volume = volume;
        musicSource2.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
