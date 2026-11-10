using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(TargetDetector))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    protected Health health;
    protected TargetDetector targetDetector;
    protected Rigidbody rb;


    [Header("Death")]
    public GameObject manaCorePrefab;
    public bool willDropCore = true;
    public float deathDelay = 1f;

    public bool hasTriggeredDeath { get; private set; }

    

    private void Start()
    {
        health = GetComponent<Health>();
        targetDetector = GetComponent<TargetDetector>();
        rb = GetComponent<Rigidbody>();

        health.corpseDuration = deathDelay;
        
    }
    protected virtual void Update()
    {
        //Detection();
        if (!hasTriggeredDeath && health.isDead)
            Die();
    }
    //private void Detection()
    //{
    //    if (confusionTimer > 0) confusionTimer -= Time.deltaTime;
    //    else isInSmoke = false;

    //    // If player has been spotted
    //    if (CanDetectPlayer())
    //        dState = DetectionState.spotted;
    //    // Default state is searching
    //    else
    //        dState = DetectionState.searching;
    //    // Being smoked overrides other two states with confusion
    //    if (isInSmoke)
    //        dState = DetectionState.confused;
    //    // Set the material to reflect the current state
    //    switch (dState)
    //    {
    //        case DetectionState.spotted:
    //            spotLight.color = spottedCol;
    //            pointLight.color = spottedCol;
    //            personalMat.color = spottedCol;
    //            healthScript.SetBaseMat(personalMat, headMRIndex);
    //            perspectivePoint.transform.LookAt(player.transform.position);
    //            break;
    //        case DetectionState.searching:
    //            spotLight.color = searchingCol;
    //            pointLight.color = searchingCol;
    //            personalMat.color = searchingCol;
    //            healthScript.SetBaseMat(personalMat, headMRIndex);
    //            break;
    //        case DetectionState.confused:
    //            spotLight.color = confusedCol;
    //            pointLight.color = confusedCol;
    //            personalMat.color = confusedCol;
    //            healthScript.SetBaseMat(personalMat, headMRIndex);
    //            break;
    //        default:
    //            break;
    //    }
    //}
    public void SmokeBomb()
    {
        targetDetector.SetConfused();
    }
    private void Die()
    {
        hasTriggeredDeath = true;
        targetDetector.spotLight.enabled = false;
        targetDetector.pointLight.enabled = false;

        if (willDropCore && manaCorePrefab != null)
            Instantiate(manaCorePrefab, transform.position, Quaternion.identity);

        rb.freezeRotation = false;
        // Gets the direction away from the player
        Vector3 randLaunchDir = (transform.position - targetDetector.target.transform.position).normalized;

        // Applies a one time force to "ragdoll" the enemy
        rb.AddForce(randLaunchDir * 50f, ForceMode.Impulse);
        Invoke(nameof(OnDeath), deathDelay);
    }
    private void OnDeath()
    {
        Destroy(gameObject);
    }
}
