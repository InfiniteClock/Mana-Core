using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [Tooltip("The layermasks to not collide with or damage")]
    public LayerMask ignoreTheseLayers;
    [Tooltip("Seconds before projectile despawns")]
    public float lifeTime;
    [Tooltip("m/s")]
    public float travelSpeed;
    [Tooltip("deg/s")]
    public float spinSpeed;
    [Tooltip("Vector axis to spin around. Normalized")]
    public Vector3 spinAxis;
    [Tooltip("Damage to enemies")]
    public float damage;
    [Tooltip("The random Z rotation that the projectile can spawn with. 0 = no rotation. Ranges from -value to +value")]
    public float randomZRotation = 0f;
    [Tooltip("Set to true if this projectile should pierce through multiple colliders")]
    public bool isPiercing = false;
    [Tooltip("Set to true if this projectile should stick to what it collides with")]
    public bool isSticking = false;
    [Tooltip("Leave null if projectile does not explode")]
    public GameObject explosionPrefab;

    [HideInInspector]
    public Vector3 origin;
    [HideInInspector]
    public bool hasFired = true;

    [Header("SFX")]
    public string launchSFX;
    public string hitSFX;
    
    private Vector3 direction;
    private float lifeTimer;
    private Rigidbody rb;
    private List<Collider> hitColliders;
    private List<Collider> selfColliders;

    protected virtual void Awake()
    {
        // Set the excluded layers for every collider in the projectile
        selfColliders = new List<Collider>();
        selfColliders = GetComponentsInChildren<Collider>().ToList<Collider>();        
        for (int i = 0; i < selfColliders.Count; i++)
        {
            selfColliders[i].excludeLayers = ignoreTheseLayers;
        }
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(launchSFX, transform);
        
        rb = GetComponent<Rigidbody>();
        hitColliders = new List<Collider>();
    }
    protected virtual void Start()
    {
        if (hasFired)
            Fire();
    }
    public void Update()
    {
        if (!hasFired) return;
        
        if (lifeTimer < lifeTime)
            lifeTimer += Time.deltaTime;
        else
            Destroy(gameObject); 
        
    }
    protected void OnTriggerEnter(Collider other)
    {
        // Return if colliding with another projectile
        if (other.gameObject.TryGetComponent(out Projectile pro)) return;

        // If this is a collider we have already hit, ignore it
        for (int i = 0; i < hitColliders.Count; i++)
        {
            if (other == hitColliders[i]) return;
        }

        // Damage objects with a health script
        if (other.gameObject.TryGetComponent(out Health health))
        {
            // Adds collider to list of colliders projectile has already hit
            hitColliders.Add(other);
            health.TakeDamage(damage);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hitSFX, transform);
        }
        
        // Damage player object's health script
        else if (other.gameObject.TryGetComponent(out PlayerHealth phealth))
        {
            hitColliders.Add(other);
            phealth.TakeDamage(origin, damage);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hitSFX, transform);
        }

        // Destroys projectile on collision with anything UNLESS it has piercing property
        if (!isPiercing)
        {
            if (explosionPrefab != null)
            {
                GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (isSticking)
            {
                ToggleFireOn(false);
                hasFired = true;
                transform.SetParent(other.transform);
            }
            else
                Destroy(gameObject);
        }
    }
    public void Fire()
    {
        // Set the direction of travel before applying any rotational forces
        direction = transform.forward;
        spinAxis = spinAxis.normalized;

        // Spawns at a random Z rotation
        Vector3 randRot = transform.rotation.eulerAngles;
        randRot.z += Random.Range(-randomZRotation, randomZRotation);
        transform.rotation = Quaternion.Euler(randRot);

        // Adds random spinning to the projectile
        spinSpeed *= Random.Range(-1f, 1f);
        rb.AddRelativeTorque(spinAxis * spinSpeed, ForceMode.Impulse);

        // Sets the projectile in motion
        rb.AddForce(direction * travelSpeed, ForceMode.Impulse);
    }
    public void ToggleFireOn(bool toggleOn)
    {
        hasFired = toggleOn;
        foreach(Collider col in selfColliders)
        {
            col.enabled = toggleOn;
        }

        rb.isKinematic = !toggleOn;
        rb.detectCollisions = toggleOn;

        // If setting to true, fire the projectile
        if (toggleOn)
            Fire();
    }
}
