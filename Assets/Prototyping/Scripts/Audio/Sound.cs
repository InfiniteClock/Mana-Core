using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip clip;

    public string name;

    [Range(0f,1f)]
    public float volume = 1f;

    [Range(-3f,3f)]
    public float pitch = 1f;

    public bool loop;

}
