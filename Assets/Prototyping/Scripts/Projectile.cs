using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("m/s")]
    public float travelSpeed;
    [Tooltip("deg/s")]
    public float spinSpeed;
    [Tooltip("Vector axis to spin around. Normalized")]
    public Vector3 spinAxis;
    [Tooltip("Meters projectile will fly before despawning")]
    public float effectiveDistance;
    [Tooltip("Damage to enemies")]
    public float damage;
    [Tooltip("True if projectile should spawn with random z rotation")]
    public bool randomRotationSpawn = false;

    [HideInInspector]
    public Vector3 fwdMomentum;

    private float lifeTime;
    private float lifeTimer;
    private Rigidbody rb;
    private void Start()
    {
        lifeTime = effectiveDistance / travelSpeed;
        spinAxis = spinAxis.normalized;

        rb = GetComponent<Rigidbody>();

        // Adds player forward velocity to projectile before applying initial impulse force to the projectiles
        rb.linearVelocity = fwdMomentum;
        rb.AddForce(transform.forward * travelSpeed, ForceMode.Impulse);

        // Spawns at a random Z rotation
        if (randomRotationSpawn)
            rb.rotation = Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));

        // Adds spinning to the projectile
        rb.AddRelativeTorque(spinAxis * spinSpeed, ForceMode.Impulse);
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

        // Damage enemies
        if (other.gameObject.TryGetComponent(out TargetDummy dummy))
        {
            dummy.TakeDamage();
        }

        Destroy(gameObject);
    }
}
