using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float travelSpeed;
    public float lifeTime;
    public float damage;

    private float lifeTimer;
    public void Update()
    {
        transform.Translate(transform.forward * travelSpeed * Time.deltaTime);

        if (lifeTimer < lifeTime)
            lifeTimer += Time.deltaTime;
        else
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<TargetDummy>(out TargetDummy dummy))
        {
            dummy.TakeDamage();
            Destroy(gameObject);
        }
    }
}
