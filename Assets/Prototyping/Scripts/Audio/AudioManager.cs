using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds;

    public List<Sound> activeSounds;

    private void Awake()
    {
        if (AudioManager.Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void PlayOneShot(string name, GameObject obj)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = s.clip;
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.loop = s.loop;
        
        source.Play();
    }

    //public int PlayInstance(string name, GameObject obj)
    //{
    //    Sound s = Array.Find(sounds, sound => sound.name == name);
    //    if (s == null)
    //        return -1;

    //    activeSounds.Add(s);

    //    s.source.Play();

    //    return activeSounds.IndexOf(s);

    //}

    //public void StopInstance(int index, GameObject obj)
    //{
    //    Sound s = activeSounds[index];
    //    s.source.Stop();
    //    activeSounds.RemoveAt(index);
    //}
}
