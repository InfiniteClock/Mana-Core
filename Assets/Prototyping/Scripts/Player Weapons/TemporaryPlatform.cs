using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TemporaryPlatform : MonoBehaviour
{
    public float platformHeight;
    public float growTime;
    public float lifeTime;
    public float breakTime;
    [Tooltip("Represents how much lifetime is removed for each 1 point of damage dealt to the platform.")]
    public float damageToTimeRatio = 1f;
    public float gravity;
    public List<Collider> colliders;

    [HideInInspector]
    public PlayerMovement player;
    [HideInInspector]
    public float launchForce;

    [Header("Launch Physics")]
    public float launchRadius;
    public Transform topCenter;
    public LayerMask launchableLayers;

    [Header("Settings")]
    public bool isFloating = false;

    private bool canTakeDamage = false;
    private float timer;
    private Vector3 fallingVelocity;
    private void Start()
    {
        // Spawned as a ground platform
        if (!isFloating)
            StartCoroutine(GrowPlatform());
        // Spawned as a floating platform
        else
        {
            // Disable all but the top part of the platform
            for (int i = 1; i < colliders.Count; i++)
            {
                colliders[i].gameObject.SetActive(false);
            }
            // Move the platform further down
            transform.position -= new Vector3 (0, platformHeight, 0);
            // Skip the grow time and go straight to counting down life time
            StartCoroutine(UpdateTime());
        }
    }
    private IEnumerator GrowPlatform()
    {
        Vector3 scale = transform.localScale;
        Vector3 minScale = transform.localScale;
        minScale.y = 0f;
        transform.localScale = minScale;
        timer = 0f;
        while (timer < growTime)
        {
            transform.localScale = Vector3.Lerp(minScale, scale, timer / growTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = scale;

        // Launches objects off of the platform when it reaches full growth
        if (Physics.SphereCast(topCenter.position, launchRadius, transform.up, out RaycastHit hitInfo, launchRadius, launchableLayers))
        {
            if (hitInfo.rigidbody != null)
            {
                hitInfo.rigidbody.AddForce(transform.up * launchForce, ForceMode.Impulse);
            }
        }

        StartCoroutine(UpdateTime());
    }
    private IEnumerator UpdateTime()
    {
        canTakeDamage = true;
        timer = lifeTime;

        while (timer > 0f)
        {
            // Show bigger cracks at 1/3rd life
            if (timer < lifeTime / 3f)
            {

            }
            // Show cracks at 2/3rds life
            else if (timer < lifeTime * 2f / 3f)
            {

            }

            timer -= Time.deltaTime;
            yield return null;
        }

        Shatter();
    }

    public void TakeDamage(float dmg)
    {
        if (canTakeDamage)
            lifeTime -= dmg * damageToTimeRatio;
    }

    private void Shatter()
    {
        // Disables all of the colliders immediately
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i].enabled = false;
        }


        // Play platform breaking animations and sounds before removing platform from world

        StartCoroutine(FallIntoVoid());

    }
    private IEnumerator FallIntoVoid()
    {
        timer = 0f;
        fallingVelocity = Vector3.zero;

        while (timer < breakTime)
        {
            // Adds gravity to the velocity of the object, clamped to 2.2x the force of gravity (rough approx of terminal velocity) then moves the transform relative to timescale
            fallingVelocity += Vector3.down * gravity * Time.deltaTime;
            fallingVelocity.y = Mathf.Clamp(fallingVelocity.y, gravity * -2.2f, 0f);
            transform.position += fallingVelocity * Time.deltaTime;

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
