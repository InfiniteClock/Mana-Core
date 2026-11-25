using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public GameObject sfxPrefab;
    [Range(0f, 1f)]
    public float masterVolCtrl = 1f;
    [Range(0f, 1f)]
    public float sfxVolCtrl = 1f;
    [Range(0f, 1f)]
    public float musicVolCtrl = 1f;
    [field:SerializeField]
    public Sound[] soundEffects { get; private set; }
    [field:SerializeField]
    public Sound[] musicTracks { get; private set; }

    private AudioSource musicSource;
    private AudioSource musicSourceFadeOut;
    private Dictionary<string, Sound> sfxDict = new();
    private Dictionary<string, Sound> musicDict = new();
    private Coroutine fadeRoutine;
    private List<AudioSource> savedSources;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savedSources = new List<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSourceFadeOut = gameObject.AddComponent<AudioSource>();

        InitializeSounds();

        musicSource.clip = musicTracks[0].clip;
        musicSource.volume = musicTracks[0].volume;
        musicSource.pitch = musicTracks[0].pitch;
        musicSource.loop = musicTracks[0].loop;
        musicSource.Play();
    }
    private void InitializeSounds()
    {
        foreach (Sound clip in soundEffects) 
            if (clip != null) sfxDict[clip.name] = clip;

        foreach (Sound clip in musicTracks)
            if (clip != null) musicDict[clip.name] = clip;
    }
    public Sound GetSFXSound(string name)
    {
        if (sfxDict.TryGetValue(name, out Sound sound))
            return sound;
        else
            return null;
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
            source.volume = sound.volume * masterVolCtrl * sfxVolCtrl;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.Play();
        }
            
    }
    public AudioSource PlayReplaceableSFX(string name, GameObject obj, Transform parent = null)
    {
        // Ensure null names are set as blank strings
        name ??= "";

        // If no parent is provided, assume to keep the original parent
        if (parent != null) obj.transform.SetParent(parent);
        

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
        else if (obj.TryGetComponent(out AudioSource source))
            return source;
        else 
            return null;
    }
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
    public void StopMusic()
    {
        musicSource.Stop();
    }
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
}
