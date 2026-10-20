using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth;
    public float corpseDuration;

    public float currentHealth { get; private set; }

    private float corpseTimer = 0f;

    [Header("Feedback")]
    public float flashInterval;
    public float flashDuration;
    public List<Material> hurtMats;

    public List<MeshRenderer> meshRenderers { get; private set; }
    public List<Material> baseMats { get; private set; }

    private bool canTakeDamage = true;
    private Coroutine flashRoutine;
    private Coroutine deathRoutine;

    private void Awake()
    {
        // Collects EVERY MeshRenderer and associated starting Material and stores for later
        MeshRenderer[] tempMR = GetComponentsInChildren<MeshRenderer>();
        meshRenderers = new List<MeshRenderer>();
        baseMats = new List<Material>();

        foreach(MeshRenderer mr in tempMR)
        {
            meshRenderers.Add(mr);
            baseMats.Add(mr.material);
        }

        // Sets health to max by default
        currentHealth = maxHealth;
    }
    public void SetBaseMat(Material newMat, int index = 0)
    {
        baseMats[index] = newMat;
        if (flashRoutine == null)
        {
            // Resets the base materials of each MeshRenderer 
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                meshRenderers[i].material = baseMats[i];
            }
        }
    }
    /// <summary>
    /// Returns the index of a meshrenderer stored in MeshRenderers list, or -1 if target cannot be found
    /// </summary>
    /// <param name="targetMR">The meshrenderer you want the index of from the list</param>
    /// <returns>The index of targetMR, or -1 if not found</returns>
    public int GetMeshRendererIndex(MeshRenderer targetMR)
    {
        int targetIndex = -1;
        if (meshRenderers.Count < 1) return -1;
        foreach(MeshRenderer mr in meshRenderers)
        {
            if (targetMR == mr) {
                targetIndex = meshRenderers.IndexOf(mr);
                break;
            }
        }
        return targetIndex;
    }
    public void TakeDamage(float dmg = 0f)
    {
        // This script returns if target cannot take damage
        if (!canTakeDamage) return;

        currentHealth -= dmg;
        Debug.Log("I have " + currentHealth + " health remaining!");
        // Only plays damage feedback if the enemy is still alive
        if (currentHealth > 0)
            flashRoutine ??= StartCoroutine(DamageFeedback(flashDuration));
        // Triggers death 
        else
            TriggerDeath();
    }
    private IEnumerator DamageFeedback(float duration)
    {
        float timer = 0f;
        int index = 0;
        while (timer < duration)
        {
            // Sets each MeshRenderer's materials to the current index's hurt material
            foreach(MeshRenderer mr in meshRenderers)
            {
                mr.material = hurtMats[index];
            }
            // Increases the index by 1, or resets to beginning if at the end
            index++;
            if (index > hurtMats.Count - 1) index = 0;

            // Delay until the next interval to change the hit box
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        // Resets the base materials of each MeshRenderer 
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].material = baseMats[i];
        }

        // Ends the routine by making it null, allowing it to be activated again
        flashRoutine = null;
    }
    protected void TriggerDeath()
    {
        canTakeDamage = false;
        deathRoutine ??= StartCoroutine(WhileDead());
    }
    protected void DeathThroes() { }
    private IEnumerator WhileDead()
    {
        while(corpseTimer < corpseDuration)
        {
            // Do things that need to happen during death here
            DeathThroes();
            corpseTimer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
