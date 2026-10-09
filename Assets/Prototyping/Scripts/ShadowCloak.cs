using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowCloak : MonoBehaviour
{
    [Header("References")]
    public bool isRightWeapon;
    public GameObject cloakFilter;
    public GameObject smokeCloudPrefab;
    private PlayerMovement player;

    [Header("Cloak")]
    public float cloakDuration;
    public float speedMulti;
    public float shadowJumpHeight;
    public float cloakCD;
    
    private float cloakCDTimer;
    private InputAction cloakInput;

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
        player = GetComponent<PlayerMovement>();
        cloakFilter.SetActive(false);
    }
    private void OnEnable()
    {
        if (isRightWeapon)
            cloakInput = InputSystem.actions.FindAction("Right Utility");
        else
            cloakInput = InputSystem.actions.FindAction("Left Utility");

        cloakInput.performed += OnCloak;
    }

    private void OnDisable()
    {
        cloakInput.performed -= OnCloak;
    }

    private void Update()
    {
        // Cloak cooldown begins when cloak stops being active
        if (cloakCDTimer > 0 && !player.isCloaked) cloakCDTimer -= Time.deltaTime;
    }

    private void Cloak()
    {
        // Returns if still on cooldown
        if (cloakCDTimer > 0) return;
        else cloakCDTimer = cloakDuration;

        // Start the smoke bomb
        StartCoroutine(Smoke());

        // Applies visual effects and sets stats of player for being cloaked
        cloakFilter.SetActive(true);
        player.isCloaked = true;
        player.cloakSpeedMulti = speedMulti + 1f;
        player.SetJumpStats(shadowJumpHeight);

        // When cloak has run out, reset the ability
        Invoke(nameof(ResetCloak), cloakDuration);
    }
    private IEnumerator Smoke()
    {
        // Spawn the smoke bomb and set its scale
        GameObject smokeBomb = Instantiate(smokeCloudPrefab);
        smokeBomb.transform.position = player.transform.position;
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
        cloakFilter.SetActive(false);
        player.isCloaked = false;
        player.cloakSpeedMulti = 1f;
        player.SetJumpStats(player.jumpHeight);
    }

    private void OnCloak(InputAction.CallbackContext context) => Cloak();
}
