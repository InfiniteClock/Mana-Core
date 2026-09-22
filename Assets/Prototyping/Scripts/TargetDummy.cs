using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    public List<Transform> patrolPoints;
    public float speed;
    public float offsetToPoint;
    public List<Material> hitFlashMaterials;
    public float flashTime;

    private int nextPoint = 0;
    private Vector3 direction;
    private Coroutine flashRoutine;
    private Material baseMaterial;
    private MeshRenderer mr;
    private void Start()
    {
        mr = GetComponent<MeshRenderer>();
        baseMaterial = mr.material;
        UpdateDirection();
    }
    private void Update()
    {
        float distanceToPoint = (patrolPoints[nextPoint].position - transform.position).magnitude;
        if (distanceToPoint < offsetToPoint)
        {
            nextPoint++;
            if (nextPoint > patrolPoints.Count - 1)
                nextPoint = 0;

            UpdateDirection();
        }

        transform.Translate(direction * speed * Time.deltaTime);
    }
    void UpdateDirection()
    {
        direction = (patrolPoints[nextPoint].position - transform.position).normalized;
        //transform.forward = direction;
    }

    public void TakeDamage()
    {
        flashRoutine ??= StartCoroutine(DamageFeedback(flashTime));
    }
    private IEnumerator DamageFeedback(float duration)
    {
        float timer = 0f;
        int index = 0;
        while (timer < duration)
        {
            mr.material = hitFlashMaterials[index];
            index++;
            if (index > hitFlashMaterials.Count - 1) index = 0;

            yield return new WaitForSeconds(flashTime);
            timer += flashTime;
        }

        mr.material = baseMaterial;
        flashRoutine = null;
    }
}
