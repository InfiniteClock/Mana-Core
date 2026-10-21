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
        SpawnSmoke();

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
    private void SpawnSmoke()
    {
        // Spawn the smoke bomb and set its scale
        GameObject smokeBomb = Instantiate(smokeCloudPrefab);
        smokeBomb.transform.position = playerMovement.transform.position;
        SmokeBomb smokeP = smokeBomb.GetComponent<SmokeBomb>();
        smokeP.lifeTime = smokeDuration;
        smokeP.diameterStart = smokeDiameterStart;
        smokeP.diameterEnd = smokeDiameterEnd;
        smokeP.smokeGrows = smokeGrows;

        if (!smokeGrows)
            smokeBomb.transform.localScale = Vector3.one * smokeDiameterStart;
        else
            smokeBomb.transform.localScale = Vector3.one * smokeDiameterEnd;
        Material mat = smokeBomb.GetComponent<MeshRenderer>().material;
        Color col = mat.color;
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
