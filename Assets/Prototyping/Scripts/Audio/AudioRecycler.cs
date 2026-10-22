using System.Collections;
using UnityEngine;

public class AudioRecycler : MonoBehaviour
{
    public float delay = 1f;

    private void Start()
    {
        StartCoroutine(Countdown());
    }
    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }
}
