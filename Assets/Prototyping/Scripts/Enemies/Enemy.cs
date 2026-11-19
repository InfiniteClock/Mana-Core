using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(TargetDetector))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public Health health { get; protected set; }
    protected TargetDetector targetDetector;
    public bool lookWithX = true;
    public bool lookWithY = true;
    public bool lookWithZ = true;
    public Transform modelParts;
    protected Rigidbody rb;


    [Header("Death")]
    public GameObject manaCorePrefab;
    public bool willDropCore = true;
    public float deathDelay = 1f;

    public bool hasTriggeredDeath { get; private set; }

    private void Awake()
    {
        health = GetComponent<Health>();
        targetDetector = GetComponent<TargetDetector>();
        rb = GetComponent<Rigidbody>();

        if (targetDetector.target == null)
            targetDetector.target = GameObject.Find("Player");
    }

    protected virtual void Start()
    {
        health.corpseDuration = deathDelay;
    }
    protected virtual void Update()
    {
        if (!hasTriggeredDeath && health.isDead)
            Die();
    }
    public void SmokeBomb()
    {
        targetDetector.SetConfused();
    }
    protected void Die()
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
    public void LookAtTarget(Transform targ)
    {
        // If we can't rotate in any direction, do not continue
        if (!lookWithX && !lookWithY && !lookWithZ) return;

        // Resets to match the agent direction
        if (targ == null) modelParts.localEulerAngles = new Vector3();

        else
        {
            // Get the intended forward direction, and remove the axises that aren't allowed to move. Then normalize.
            Vector3 lookDirection = (targ.position - transform.position);
            if (!lookWithX) lookDirection.x = 0f;
            if (!lookWithY) lookDirection.y = 0f;
            if (!lookWithZ) lookDirection.z = 0f;
            lookDirection.Normalize();

            // Set the rotation of the moveable parts, without touching the rest of the enemy
            modelParts.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
