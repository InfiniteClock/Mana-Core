using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
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

    
    private Vector3 direction;
    private float speed;
    private float lifeTimer;
    private Rigidbody rb;
    private List<Collider> hitColliders;

    private void Start()
    {
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
    private void OnTriggerEnter(Collider other)
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
        }

        // Destroys projectile on collision with anything UNLESS it has piercing property
        if (!isPiercing)
            Destroy(gameObject);
    }
}
