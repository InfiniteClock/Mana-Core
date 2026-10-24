using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [Tooltip("The layermasks to not collide with or damage")]
    public LayerMask ignoreTheseLayers;
    [Tooltip("Seconds before projectile despawns")]
    public float lifeTime;
    [Tooltip("m/s")]
    public float travelSpeed;
    [Tooltip("The curve at which the projectile applies travelSpeed over lifetime")]
    public AnimationCurve speedCurve;
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
    public Vector3 origin;

    [Header("SFX")]
    public string launchSFX;
    public string hitSFX;
    
    private Vector3 direction;
    private float speed;
    private float lifeTimer;
    private Rigidbody rb;
    private List<Collider> hitColliders;
    private List<Collider> selfColliders;

    protected virtual void Start()
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
        
        spinAxis = spinAxis.normalized;
        rb = GetComponent<Rigidbody>();
        hitColliders = new List<Collider>();

        // Set the direction of travel before applying any rotational forces
        direction = transform.forward;

        // Spawns at a random Z rotation
        Vector3 randRot = transform.rotation.eulerAngles;
        randRot.z += Random.Range(-randomZRotation, randomZRotation);
        transform.rotation = Quaternion.Euler(randRot);
        
        // Adds random spinning to the projectile
        spinSpeed *= Random.Range(-1f, 1f);
        rb.AddRelativeTorque(spinAxis * spinSpeed, ForceMode.Impulse);
    }
    private void FixedUpdate()
    {
        speed = speedCurve.Evaluate(lifeTimer / lifeTime) * travelSpeed;
        rb.linearVelocity = direction * speed;
    }
    public void Update()
    {
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
            Destroy(gameObject);
    }
}
