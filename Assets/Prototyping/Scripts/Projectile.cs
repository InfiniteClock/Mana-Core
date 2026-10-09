using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float travelSpeed;
    public float lifeTime;
    public float damage;

    [HideInInspector]
    public float fwdMomentum;
    private float lifeTimer;
    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = fwdMomentum * transform.forward;
        rb.AddForce(transform.forward * travelSpeed, ForceMode.Impulse);
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
        if (other.gameObject.TryGetComponent<TargetDummy>(out TargetDummy dummy))
        {
            dummy.TakeDamage();
            Destroy(gameObject);
        }
    }
}
