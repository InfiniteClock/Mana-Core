using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float travelSpeed;
    public float lifeTime;
    public float damage;

    private float lifeTimer;
    public void Update()
    {
        transform.Translate((travelSpeed * Time.deltaTime) * transform.forward, Space.World);

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
