using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public GameObject sfxPrefab;
    [Range(0.01f, 1f)]
    public float masterVolCtrl = 1f;
    public Slider masterVolume;
    [Range(0.01f, 1f)]
    public float sfxVolCtrl = 1f;
    public Slider sfxVolume;
    [Range(0.01f, 1f)]
    public float musicVolCtrl = 1f;
    public Slider musicVolume;
    [field:SerializeField]
    public Sound[] soundEffects { get; private set; }
    [field:SerializeField]
    public Sound[] musicTracks { get; private set; }


    private float lastMasterVol;
    private float lastSFXVol;
    private float lastMusicVol;
    private AudioSource musicSource;
    private AudioSource musicSourceFadeOut;
    private Dictionary<string, Sound> sfxDict = new();
    private Dictionary<string, Sound> musicDict = new();
    private Coroutine fadeRoutine;
    public List<AudioSource> savedSources;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savedSources = new List<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSourceFadeOut = gameObject.AddComponent<AudioSource>();

        masterVolume.value = masterVolCtrl;
        sfxVolume.value = sfxVolCtrl;
        musicVolume.value = musicVolCtrl;

        InitializeSounds();

        musicSource.clip = musicTracks[0].clip;
        musicSource.volume = musicTracks[0].volume;
        musicSource.pitch = musicTracks[0].pitch;
        musicSource.loop = musicTracks[0].loop;
        musicSource.Play();

        lastMasterVol = masterVolCtrl;
        lastSFXVol = sfxVolCtrl;
        lastMusicVol = musicVolCtrl;
    }
    /// <summary>
    /// Initializes the dictionaries of Sounds on this object
    /// </summary>
    private void InitializeSounds()
    {
        foreach (Sound clip in soundEffects) 
            if (clip != null) sfxDict[clip.name] = clip;

        foreach (Sound clip in musicTracks)
            if (clip != null) musicDict[clip.name] = clip;
    }
    /// <summary>
    /// Provides the Sound data for a specified sound file
    /// </summary>
    /// <param name="name">The name of the sound file</param>
    /// <returns>The Sound file containing its data (read-only)</returns>
    public Sound GetSFXSound(string name)
    {
        if (sfxDict.TryGetValue(name, out Sound sound))
            return sound;
        else
            return null;
    }
    /// <summary>
    /// Plays a one-time SFX
    /// </summary>
    /// <param name="name">The name of the sound file to play</param>
    /// <param name="parent">The parent, if any, that this AudioSource should play from in the world</param>
    public void PlaySFX(string name, Transform parent = null)
    {
        // If no parent is provided, use Audio Manager as the parent
        if (parent == null) parent = transform;

        // If we can find the audio file by name, then play that file
        if (sfxDict.TryGetValue(name, out Sound sound))
        {
            GameObject newSource = Instantiate(sfxPrefab, parent);
            AudioSource source = newSource.GetComponent<AudioSource>();
            AudioRecycler rec = newSource.GetComponent<AudioRecycler>();

            newSource.name = name;
            rec.delay = sound.clip.length;
            source.clip = sound.clip;
            source.volume = sound.volume * masterVolCtrl * sfxVolCtrl;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.Play();

            savedSources.Add(source);
        }
            
    }
    /// <summary>
    /// Sets up an existing AudioSource on an object with the specified sound file 
    /// </summary>
    /// <param name="name">The audio file</param>
    /// <param name="obj">The game object that has the source on it</param>
    /// <param name="parent">The parent object to attach the audio source object to</param>
    /// <returns>The AudioSource that has been set up</returns>
    public AudioSource PlayReplaceableSFX(string name, GameObject obj, Transform parent = null)
    {
        // Ensure null names are set as blank strings
        name ??= "";

        // If no parent is provided, assume to keep the original parent
        if (parent != null) obj.transform.SetParent(parent);
        
        // If we find the specified audio file in our dict...
        if (sfxDict.TryGetValue(name, out Sound sound))
        {
            AudioSource source = obj.GetComponent<AudioSource>();
            AudioRecycler rec = obj.GetComponent<AudioRecycler>();

            source.Stop();
            rec.enabled = false;
            source.clip = sound.clip;
            source.volume = sound.volume * masterVolCtrl * sfxVolCtrl;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.Play();

            return source;
        }
        // Couldn't find audio file but the given object has an audio source...
        else if (obj.TryGetComponent(out AudioSource source))
            return source;
        // Couldn't find audio file or object's audio source, so return null
        else 
            return null;
    }
    /// <summary>
    /// Sets up and plays the specified music file
    /// </summary>
    /// <param name="name">The name of the sound file</param>
    public void PlayMusic(string name)
    {
        if (musicDict.TryGetValue(name, out Sound sound))
        {
            musicSource.clip = sound.clip;
            musicSource.volume = sound.volume * masterVolCtrl * musicVolCtrl;
            musicSource.pitch = sound.pitch;
            musicSource.loop = sound.loop;
            musicSource.Play();
        }
    }
    /// <summary>
    /// Immediately stops any and all music sources
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
        musicSourceFadeOut.Stop();
    }
    /// <summary>
    /// Fades one music source out and a new one in
    /// </summary>
    /// <param name="name">The name of the sound file to fade-in as the new music</param>
    /// <param name="fadeTime">The duration in seconds for the fade to complete</param>
    public void FadeInNewMusic(string name, float fadeTime)
    {
        if (musicDict.TryGetValue(name, out Sound sound))
        {
            // Stop the fade routine early, and start it up again with the new track
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);

            // Set the current music source to be the fadeout source
            musicSourceFadeOut.clip = musicSource.clip;
            musicSourceFadeOut.volume = musicSource.volume;
            musicSourceFadeOut.pitch = musicSource.pitch;
            musicSourceFadeOut.loop = musicSource.loop;

            // Replace the current music source with 
            musicSource.clip = sound.clip;
            musicSource.volume = sound.volume * masterVolCtrl * musicVolCtrl;
            musicSource.pitch = sound.pitch;
            musicSource.loop = sound.loop;

            // Begin the fade over time
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
            // Rebalance the new clip to be louder and the old clip to be quieter over time
            musicSource.volume = musicVolume * (1f - timer / fadeTime) * masterVolCtrl * musicVolCtrl;
            musicSourceFadeOut.volume = musicVolume2 * timer / fadeTime * masterVolCtrl * musicVolCtrl;

            timer += Time.deltaTime;
            yield return null;
        }
        // Set the sources to their final volume values
        musicSource.volume = musicVolume * masterVolCtrl * musicVolCtrl;
        musicSourceFadeOut.volume = 0f;

        // Clear the fade out channel of its clip
        musicSourceFadeOut.clip = null;
    }

    public void UpdateMasterVolume()
    {
        masterVolCtrl = masterVolume.value;

        // Adjusts both music and sfx audio sources by the master system
        for (int i = 0; i < savedSources.Count; i++)
        {
            savedSources[i].volume /= lastMasterVol;
            savedSources[i].volume *= masterVolCtrl / lastMasterVol; 
        }
        musicSource.volume /= lastMasterVol;
        musicSource.volume *= masterVolCtrl;

        lastMasterVol = masterVolCtrl;
    }
    public void UpdateSFXVolume()
    {
        sfxVolCtrl = sfxVolume.value;

        // Adjusts every existing saved audio source to change volume when the master sliders are adjusted
        for (int i = 0; i < savedSources.Count; i++)
        {
            savedSources[i].volume /= lastSFXVol;
            savedSources[i].volume *= sfxVolCtrl;
        }

        lastSFXVol = sfxVolCtrl;
    }
    public void UpdateMusicVolume()
    {
        musicVolCtrl = musicVolume.value;

        // Don't need to adjust the fade out music source because it is only ever active while under frame-by-frame updates anyway
        musicSource.volume /= lastMusicVol;
        musicSource.volume *= musicVolCtrl;

        lastMusicVol = musicVolCtrl;
    }
}
