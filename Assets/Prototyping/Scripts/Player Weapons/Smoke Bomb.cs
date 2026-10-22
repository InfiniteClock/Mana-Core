using System.Collections;
using UnityEngine;

public class SmokeBomb : Projectile
{
    public float diameterStart;
    public float diameterEnd;
    public bool smokeGrows;

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.SmokeBomb();
        }
    }

    protected override void Start()
    {
        base.Start();
        if (smokeGrows)
            StartCoroutine(SmokeLife());
    }

    private IEnumerator SmokeLife()
    {
        float timer = 0f;

        while (timer < lifeTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(diameterStart, diameterEnd, timer / lifeTime);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one * diameterEnd;
    }
}
