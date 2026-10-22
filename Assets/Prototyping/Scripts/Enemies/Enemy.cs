using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Health))]
public class Enemy : MonoBehaviour
{
    public enum DetectionState { searching, spotted, confused }

    private Rigidbody rb;

    [Header("Player Detection")]
    public PlayerMovement player;
    public Transform perspectivePoint;
    [Min(1f)]
    public float maxDetectionRange;
    [Min(0f)]
    public float minDetectionRange;
    [Range(0f, 180f)]
    public float detectionAngle;
    public LayerMask sightBlockingMask;

    public DetectionState dState { get; protected set; }

    [Header("Feedback")]
    public Light spotLight;
    public Light pointLight;
    public Color searchingCol;
    public Color spottedCol;
    public Color confusedCol;
    public bool isInSmoke;
    public float confusionDuration;

    private Health healthScript;
    private MeshRenderer headMR;
    private Material personalMat;
    private int headMRIndex = -1;
    private float confusionTimer;

    [Header("Death")]
    public GameObject manaCorePrefab;
    public bool willDropCore = true;
    public float deathDelay = 1f;

    public bool hasTriggeredDeath { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!TryGetComponent<Health>(out healthScript))
            Debug.Log("No Health script attached to " + gameObject.name);
        else
        {
            headMR = perspectivePoint.GetComponent<MeshRenderer>();
            headMRIndex = healthScript.GetMeshRendererIndex(headMR);
            personalMat = headMR.material;
            healthScript.corpseDuration = deathDelay;
        }
        spotLight.range = maxDetectionRange;
        spotLight.spotAngle = detectionAngle;
        spotLight.innerSpotAngle = detectionAngle;
        pointLight.range = minDetectionRange;
    }
    private void OnValidate()
    {
        spotLight.range = maxDetectionRange;
        spotLight.spotAngle = detectionAngle;
        spotLight.innerSpotAngle = detectionAngle;
        pointLight.range = minDetectionRange;
    }
    protected virtual void Update()
    {
        Detection();
        if (!hasTriggeredDeath && healthScript.isDead)
            Die();
    }
    private void Detection()
    {
        if (confusionTimer > 0) confusionTimer -= Time.deltaTime;
        else isInSmoke = false;

        // If player has been spotted
        if (CanDetectPlayer())
            dState = DetectionState.spotted;
        // Default state is searching
        else
            dState = DetectionState.searching;
        // Being smoked overrides other two states with confusion
        if (isInSmoke)
            dState = DetectionState.confused;
        // Set the material to reflect the current state
        switch (dState)
        {
            case DetectionState.spotted:
                spotLight.color = spottedCol;
                pointLight.color = spottedCol;
                personalMat.color = spottedCol;
                healthScript.SetBaseMat(personalMat, headMRIndex);
                perspectivePoint.transform.LookAt(player.transform.position);
                break;
            case DetectionState.searching:
                spotLight.color = searchingCol;
                pointLight.color = searchingCol;
                personalMat.color = searchingCol;
                healthScript.SetBaseMat(personalMat, headMRIndex);
                break;
            case DetectionState.confused:
                spotLight.color = confusedCol;
                pointLight.color = confusedCol;
                personalMat.color = confusedCol;
                healthScript.SetBaseMat(personalMat, headMRIndex);
                break;
            default:
                break;
        }
    }
    private bool CanDetectPlayer()
    {
        // If the player is cloaked, must still be searching
        if (player.isCloaked) return false;

        // Get distance to player
        Vector3 toPlayer = player.transform.position - perspectivePoint.position;
        float distToPlayer = toPlayer.magnitude;

        // If player is outside of max detection range, must be in searching still
        if (distToPlayer > maxDetectionRange) return false;

        // If the player is not within the min radius or the sightline angle of the max radius, must still be searching
        float angle = Vector3.Angle(perspectivePoint.transform.forward, toPlayer);
        if (angle > detectionAngle / 2f && distToPlayer > minDetectionRange) return false;

        // If the sightline to the player is NOT blocked by terrain, spot them
        return !Physics.Raycast(perspectivePoint.position, toPlayer.normalized, distToPlayer, sightBlockingMask);

    }
    public void SmokeBomb()
    {
        if (!isInSmoke) isInSmoke = true;
        if (confusionTimer <= 0) confusionTimer = confusionDuration;
    }
    private void Die()
    {
        hasTriggeredDeath = true;
        spotLight.enabled = false;
        pointLight.enabled = false;

        if (willDropCore && manaCorePrefab != null)
            Instantiate(manaCorePrefab, transform.position, Quaternion.identity);

        rb.freezeRotation = false;
        // Gets the direction away from the player
        Vector3 randLaunchDir = (transform.position - player.transform.position).normalized;

        // Applies a one time force to "ragdoll" the enemy
        rb.AddForce(randLaunchDir * 50f, ForceMode.Impulse);
        Invoke(nameof(OnDeath), deathDelay);
    }
    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draws max radius of detection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(perspectivePoint.position, maxDetectionRange);

        // Draws minimum area of detection
        Gizmos.color = Color.wheat;
        Gizmos.DrawWireSphere(perspectivePoint.position, minDetectionRange);

        // Draws angle between min and max radius of detection
        Vector3 rightSideLine = Quaternion.Euler(0f, detectionAngle / 2f, 0f) * perspectivePoint.transform.forward;
        Vector3 leftSideLine = Quaternion.Euler(0f, -detectionAngle / 2f, 0f) * perspectivePoint.transform.forward;
        Gizmos.DrawLine(perspectivePoint.position + rightSideLine * minDetectionRange, perspectivePoint.position + rightSideLine * maxDetectionRange);
        Gizmos.DrawLine(perspectivePoint.position + leftSideLine * minDetectionRange, perspectivePoint.position + leftSideLine * maxDetectionRange);
    }

}
