using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public GameObject sfxPrefab;
    public Sound[] soundEffects;
    public Sound[] musicTracks;

    private AudioSource musicSource;
    private AudioSource musicSourceFadeOut;
    private Dictionary<string, Sound> sfxDict = new();
    private Dictionary<string, Sound> musicDict = new();
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSourceFadeOut = gameObject.AddComponent<AudioSource>();

        InitializeSounds();
    }
    private void InitializeSounds()
    {
        foreach (Sound clip in soundEffects) 
            if (clip != null) sfxDict[clip.name] = clip;

        foreach (Sound clip in musicTracks)
            if (clip != null) musicDict[clip.name] = clip;
    }

    public void PlaySFX(string name, Transform parent = null)
    {
        // If no parent is provided, use Audio Manager as the parent
        if (parent == null) parent = transform;

        if (sfxDict.TryGetValue(name, out Sound sound))
        {
            GameObject newSource = Instantiate(sfxPrefab, parent);
            AudioSource source = newSource.GetComponent<AudioSource>();
            AudioRecycler rec = newSource.GetComponent<AudioRecycler>();

            newSource.name = name;
            rec.delay = sound.clip.length;
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.Play();
        }
            
    }
    public void PlayMusic(string name)
    {
        if (musicDict.TryGetValue(name, out Sound sound))
        {
            musicSource.clip = sound.clip;
            musicSource.volume = sound.volume;
            musicSource.pitch = sound.pitch;
            musicSource.loop = sound.loop;
            musicSource.Play();
        }
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }
    public void FadeInNewMusic(string name, float fadeTime)
    {
        if (musicDict.TryGetValue(name, out Sound sound))
        {
            musicSourceFadeOut.clip = sound.clip;
            musicSourceFadeOut.volume = sound.volume;
            musicSourceFadeOut.pitch = sound.pitch;
            musicSourceFadeOut.loop = sound.loop;

            // Stop the fade routine early, and start it up again with the new track
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            StartCoroutine(FadeMusic(fadeTime));
        }
    }

    private IEnumerator FadeMusic(float fadeTime)
    {
        float timer = 0f;
        float musicVolume = musicSource.volume;
        float musicVolume2 = musicSourceFadeOut.volume;

        while (timer < fadeTime)
        {
            musicSource.volume = musicVolume * (1f - timer / fadeTime);
            musicSourceFadeOut.volume = musicVolume2 * timer / fadeTime;

            timer += Time.deltaTime;
            yield return null;
        }

        musicSource = musicSourceFadeOut;
        musicSourceFadeOut.clip = null;
    }
}
