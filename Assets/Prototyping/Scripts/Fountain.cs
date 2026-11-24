using UnityEngine;

public class Fountain : MonoBehaviour
{
    public float updatesPerSec = 1.0f;

    [Header("Player Respawning")]
    public Transform respawnPoint;

    [Header("Core Spawning")]
    public float coreSpawnTime;
    public bool spawnOnAwake = true;
    public Transform manaCoreSpawnPoint;
    public GameObject manaCorePrefab;

    private GameObject spawnedManaCore;

    private void Start()
    {
        if (spawnOnAwake) SpawnCore();
        SlowUpdate();
    }
    private void SlowUpdate()
    {
        if (spawnedManaCore == null) Invoke(nameof(SpawnCore), coreSpawnTime);

        Invoke(nameof(SlowUpdate), updatesPerSec);
    }
    private void SpawnCore()
    {
        // Remove an existing core if the respawn is triggered again
        if (spawnedManaCore != null) Destroy(spawnedManaCore);

        GameObject core = Instantiate(manaCorePrefab, manaCoreSpawnPoint);
        spawnedManaCore = core;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerHealth pHealth))
        {
            // Set the player's checkpoint
            pHealth.SetCheckpoint(this);
        }
    }
}
