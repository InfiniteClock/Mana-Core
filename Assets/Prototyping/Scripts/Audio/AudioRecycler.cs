using System.Collections;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioRecycler : MonoBehaviour
{
    public float delay = 1f;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(Countdown());
    }
    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(delay);

        AudioManager.Instance.savedSources.Remove(audioSource);

        Destroy(gameObject);
    }
}
