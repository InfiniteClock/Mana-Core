using System.Collections.Generic;
using UnityEngine;

public class IcePlatform : Ability
{
    [Header("Platform Summoning")]
    public GameObject platformPrefab;
    public float summonTime;
    public float launchForce;
    public float platformLifeTime;

    [HideInInspector]
    public Vector3 spawnPosition;

    private List<TemporaryPlatform> spawnedPlatforms;

    [Header("Settings")]
    public bool launchesPlayer;
    public float maxPlatformsInWorld;

    private void Start()
    {
        spawnedPlatforms = new List<TemporaryPlatform>();
    }

    public void SpawnPlatform(Vector3 spawnpoint, Vector3 upwards, bool isFloating = false)
    {
        Debug.Log("Spawning " + (isFloating ? "floating " : "") + "platform at: " + spawnpoint.ToString());

        // Returns if ability has no remaining charges or is on internal buffer
        if (playerWeapons.GetAbilityCharges(weapon.isRightWeapon) < 1 || playerWeapons.bufferTimer > 0) return;

        InitiateAbility();

        // Check through list to remove any missing platforms
        for (int i = 0; i < spawnedPlatforms.Count; i++)
        {
            if (spawnedPlatforms[i] == null) spawnedPlatforms.RemoveAt(i);
        }

        // If we have as many platforms as we can, shatter the oldest one and remove it now
        if (spawnedPlatforms.Count >= maxPlatformsInWorld)
        {
            spawnedPlatforms[0].TakeDamage(1000f);
            spawnedPlatforms.RemoveAt(0);
        }

        GameObject platform = Instantiate(platformPrefab, spawnpoint, Quaternion.Euler(upwards));
        TemporaryPlatform plat = platform.GetComponent<TemporaryPlatform>();
        plat.player = playerMovement;
        plat.launchForce = launchForce;
        plat.growTime = summonTime;
        plat.lifeTime = platformLifeTime;
        plat.isFloating = isFloating;

        spawnedPlatforms.Add(plat);
    }

    private void OnDestroy()
    {
        spawnedPlatforms.Clear();
    }
}
