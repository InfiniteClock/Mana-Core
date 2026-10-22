using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Transform orientation;

    [Header("Health Stats")]
    public float maxHealth;
    public float currentHealth;
    public float iFramesDuration;

    public bool isDead { get; private set; }

    [Header("Feedback")]
    public Slider healthBar;
    public Transform damageMarkerParent;
    public GameObject damageMarkerPrefab;
    [Tooltip("The duration in seconds of the hitmarker, per point of damage taken")]
    public float damageMarkerDuration;
    public AnimationCurve fadeAwayCurve;

    private float iFrameTimer;

    private void Start()
    {
        healthBar.maxValue = maxHealth;

        // Set current health to max by default
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
    }
    private void Update()
    {
        if (iFrameTimer < iFramesDuration)
            iFrameTimer += Time.deltaTime;
    }
    public void TakeDamage(Transform dmgOrigin, float dmg = 0f)
    {
        // Can't take damage during i-frames or while dead
        if (iFrameTimer < iFramesDuration || isDead) return;
        else
            iFrameTimer = 0f;

        currentHealth -= dmg;
        healthBar.value = currentHealth;

        StartCoroutine(DamageMarker(dmgOrigin, dmg));

        if (currentHealth < 0f)
            TriggerDeath();
        
    }
    private IEnumerator DamageMarker(Transform dmgOrigin, float dmg)
    {
        float lifeTime = damageMarkerDuration * dmg;
        float timer = 0f;

        GameObject marker = Instantiate(damageMarkerPrefab, damageMarkerParent);
        Image img = marker.GetComponent<Image>();
        Color markerCol = img.color;


        while (timer < lifeTime)
        {
            Vector3 a = orientation.forward;
            Vector3 b = dmgOrigin.position - orientation.position;

            float dirToOrigin = Vector3.Angle(a, b);
            dirToOrigin *= Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(a, b))) * -1f;
            marker.transform.rotation = Quaternion.Euler(0f, 0f, dirToOrigin);

            markerCol.a = fadeAwayCurve.Evaluate(timer / lifeTime);
            img.color = markerCol;

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(marker);
    }
    private void TriggerDeath()
    {
        isDead = true;
    }
}
