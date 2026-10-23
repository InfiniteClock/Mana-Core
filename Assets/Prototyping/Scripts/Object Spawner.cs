using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject spawnableObj;
    public Transform spawnPoint;
    public int maxObjsInWorld;
    public float delayBeforeSpawning;
    public bool spawnOnAwake;
    public List<GameObject> spawnedObjects;

    

    private void Start()
    {
        if (spawnOnAwake) SpawnObj();
        SlowUpdate();
    }
    private void SlowUpdate()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAll(x => x == null);
                //spawnedObjects.RemoveAt(i);
                Invoke(nameof(SpawnObj), delayBeforeSpawning);
            }
        }

        // Runs this method once per second
        Invoke(nameof(SlowUpdate), 1.0f);
    }
    private void SpawnObj()
    {
        GameObject obj = Instantiate(spawnableObj, spawnPoint.position, spawnPoint.rotation);
        spawnedObjects.Add(obj);
        
        if (spawnedObjects.Count < maxObjsInWorld)
            Invoke(nameof(SpawnObj), delayBeforeSpawning);
    }
}
