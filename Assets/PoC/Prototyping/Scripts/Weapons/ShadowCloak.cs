using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ShadowCloak : Ability
{
    [Header("VFX Controller")]
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private string invisibilityFeatureName = "Invisibility VFX";
    private ScriptableRendererFeature invisibilityVFX;
    
    [Header("References")]
    public bool isRightWeapon;
    public GameObject smokeCloudPrefab;

    [Header("Cloak")]
    public float cloakDuration;
    public float speedMulti;
    public float cloakGravMod;
    public float cloakAirMod;

    [Header("Smoke")]
    public float smokeDuration;
    public float smokeDiameterStart;
    public float smokeDiameterEnd;
    public float smokeTransparencyStart;
    public float smokeTransparencyEnd;

    [Header("Settings")]
    public bool smokeGrows;
    public bool boostsAirControl;
    public bool boostsOtherAbilities;

    private void Start()
    {

        if (rendererData != null)
            {
               foreach (var feature in rendererData.rendererFeatures)
               {
                   if (feature.name == invisibilityFeatureName)
                   {
                       invisibilityVFX = feature;
                       break;
                    }
                }
             }
    }

    public void Cloak()
    {
        // Returns if dash has no remaining charges or is on internal buffer from other weapon
        if (playerWeapons.GetAbilityCharges(weapon.isRightWeapon) < 1 || playerWeapons.bufferTimer > 0) return;
        else
        {
            // Reset the cooldown, 
            cooldownTimer = cooldown;
            playerWeapons.UseAbilityCharge(weapon.isRightWeapon);

            if (playerWeapons.hasTwinWeapons)
                playerWeapons.bufferTimer = playerWeapons.dashBuffer;
        }

        // Start the smoke bomb
        StartCoroutine(Smoke());

        // Applies visual effects and sets stats of player for being cloaked
        invisibilityVFX.SetActive(true);
        playerMovement.isCloaked = true;
        playerMovement.cloakSpeedMulti = speedMulti;
        playerMovement.cloakGravMod = cloakGravMod;

        if (boostsAirControl)
            playerMovement.cloakAirMod = cloakAirMod;

        // When cloak has run out, reset the ability
        Invoke(nameof(ResetCloak), cloakDuration);
    }
    private IEnumerator Smoke()
    {
        // Spawn the smoke bomb and set its scale
        GameObject smokeBomb = Instantiate(smokeCloudPrefab);
        smokeBomb.transform.position = playerMovement.transform.position;
        if (!smokeGrows)
            smokeBomb.transform.localScale = Vector3.one * smokeDiameterStart;
        else
            smokeBomb.transform.localScale = Vector3.one * smokeDiameterEnd;
        Material mat = smokeBomb.GetComponent<MeshRenderer>().material;
        Color col = mat.color;

        float timer = 0f;

        while (timer < smokeDuration)
        {
            // Grow the smoke bomb over its duration
            if (smokeGrows)
                smokeBomb.transform.localScale = Vector3.one * Mathf.Lerp(smokeDiameterStart, smokeDiameterEnd, timer / smokeDuration);

            // Smoke fades over its duration
            col = new Color(col.r, col.g, col.b, Mathf.Lerp(smokeTransparencyStart, smokeTransparencyEnd, timer/smokeDuration));
            mat.color = col;

            timer += Time.deltaTime;
            yield return null;
        }
        smokeBomb.transform.localScale = Vector3.one * smokeDiameterEnd;

        // Despawn the smoke bomb at the end of its life
        Destroy(smokeBomb);
    }
    private void ResetCloak()
    {
        // Resets the visual effects and stats of player back to normal
        invisibilityVFX.SetActive(false);
        playerMovement.isCloaked = false;
        playerMovement.cloakSpeedMulti = 1f;
        playerMovement.cloakGravMod = 1f;
        playerMovement.cloakAirMod = 1f;
    }

    private void OnCloak(InputAction.CallbackContext context) => Cloak();
}
