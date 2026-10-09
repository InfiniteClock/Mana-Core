using TMPro;
using UnityEngine;
using System.Collections.Generic;


public abstract class Weapon : MonoBehaviour
{
    public enum WeaponType { Shadow, Wind, Fire, Ice, Earth, Lightning, Empty }

    [Header("Shooting")]
    public GameObject projectile;

    [HideInInspector]
    public Vector3 target;
    [HideInInspector]
    public Vector3 playerVelocity;

    [Header("Energy")]
    public float maxAmmo;
    public float ammoPerShot;

    protected float currentAmmo;

    [Header("Input & Display")]
    public bool inRightHand;
    public TextMeshProUGUI ammoCounter;
    public Transform projectileSpawnPoint;
    public List<RectTransform> reticle;
    [Tooltip("Assign a Vector2 direction for the reticle to move during bloom")]
    public List<Vector3> reticleDirection;

    protected List<Vector3> reticlePositions;

    [Header("Bloom")]
    [Tooltip("Base spread in degrees")]
    public float baseSpread;
    [Tooltip("How much bloom is added per shot in degrees")]
    public float bloomPerShot;
    [Tooltip("Maximum additional bloom in degrees")]
    public float maxBloom;
    [Tooltip("The distance the reticle moves to signify max bloom in pixels")]
    public float maxBloomReticleOffset;
    [Tooltip("Speed at which bloom returns to minimum in degrees per second")]
    public float bloomDecayPerSec;

    protected float bloom;

    [Header("Movement Multipliers")]
    public float standingMulti = 1.0f;
    public float movingMulti;
    public float movement01;

    [Header("Settings")]
    public bool usingCircularDistribution = true;


    private void Start()
    {
        currentAmmo = maxAmmo * 0.9f;

        if (reticleDirection.Count != reticle.Count)
            Debug.LogError("Improper reticle setup on: " + gameObject.name);

        for(int i = 0; i < reticle.Count; i++)
        {
            reticlePositions[i] = reticle[i].position;
        }
    }
    public void EquipToHand(bool rightHand)
    {
        inRightHand = rightHand;
    }
    protected void UpdateAmmo()
    {
        ammoCounter.text = currentAmmo.ToString("0") + "/" + maxAmmo.ToString("0");
    }
    private void Update()
    {
        BloomTick(Time.deltaTime);
    }
    private void BloomTick(float deltaTime)
    {
        bloom = Mathf.MoveTowards(bloom, 0f, bloomDecayPerSec * deltaTime);
        for (int i = 0; i < reticle.Count; i++)
        {
            reticle[i].position = Vector3.Lerp(reticlePositions[i], reticlePositions[i] + reticleDirection[i] * maxBloomReticleOffset, (baseSpread + bloom) / (baseSpread + maxBloom));
        }
    }
    protected void BloomOnShotFired(float bloomMulti = 1.0f)
    {
        bloom = Mathf.Clamp(bloom + bloomPerShot * bloomMulti, 0f, maxBloom);
    }
    protected float GetCurrentSpreadDegrees(float selectedMulti, float movement01)
    {
        float moveMulti = Mathf.Lerp(standingMulti, selectedMulti, Mathf.Clamp01(movement01));

        float total = (baseSpread + bloom) * moveMulti;
        return Mathf.Max(0f, total);

    }
    protected Quaternion GetSpreadRotation(float spreadDegrees)
    {
        if (spreadDegrees <= 0.0001f)
            return Quaternion.identity;

        if (usingCircularDistribution)
        {
            // Circular (Or cone) distribution feels better than square distribution
            Vector2 r = Random.insideUnitCircle * spreadDegrees;
            return Quaternion.Euler(r.y, r.x, 0f);
        }
        else
        {
            // Square distribution feels wider on angles but could be preferable in some cases
            float yaw = Random.Range(-spreadDegrees, spreadDegrees);
            float pitch = Random.Range(-spreadDegrees, spreadDegrees);
            return Quaternion.Euler(pitch, yaw, 0f);

        }

    }
    public abstract void PrimaryBegin();
    public abstract void PrimaryRelease();
    public abstract void SecondaryBegin();
    public abstract void SecondaryRelease();
}
