using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(FlyerWeapon))]
[RequireComponent(typeof(NavMeshAgent))]
public class FlyerEnemy : Enemy
{
    public float maxHeight;
    public float minHeight;
    public float rollTime;
    public float rollResistance;

    private float agentRadius;
    private Coroutine rollRoutine;
    private Coroutine heightCheckRoutine;
    private FlyerWeapon fWeapon;
    private NavMeshAgent meshAgent;

    protected override void Start()
    {
        base.Start();
        fWeapon = GetComponent<FlyerWeapon>();

        meshAgent = GetComponent<NavMeshAgent>();
        agentRadius = meshAgent.radius;
    }
    public void ChangeHeight(float additionalHeight)
    {
        // Clamp the additional height to keep flyer within height restrictions
        if (meshAgent.baseOffset + additionalHeight > maxHeight)
            additionalHeight = maxHeight - meshAgent.baseOffset;
        if (meshAgent.baseOffset + additionalHeight < minHeight)
            additionalHeight = meshAgent.baseOffset - minHeight;

        // Check that there is nothing in the way
        // If going up...
        if (additionalHeight > 0)
        {
            if (Physics.CapsuleCast(transform.position, transform.position + Vector3.up * (additionalHeight + agentRadius), agentRadius, Vector3.up, out RaycastHit hit))
                additionalHeight = hit.distance;
        }
        // If going down...
        else if (additionalHeight < 0)
        {
            if (Physics.CapsuleCast(transform.position, transform.position + Vector3.up * (additionalHeight - agentRadius), agentRadius, Vector3.up, out RaycastHit hit))
                additionalHeight = -hit.distance;
        }

        if (heightCheckRoutine != null)
        {
            StopCoroutine(heightCheckRoutine);
            heightCheckRoutine = null;
        }

        heightCheckRoutine ??= StartCoroutine(AdjustHeight(additionalHeight));
    }
    private IEnumerator AdjustHeight(float newHeight)
    {
        float speed = meshAgent.speed;
        float startHeight = meshAgent.baseOffset;
        float timer = 0f;
        float timeToHeight = newHeight / speed;
        
        while (timer < timeToHeight)
        {
            float currentHeight = Mathf.Lerp(startHeight, startHeight + newHeight, timer / timeToHeight);
            meshAgent.baseOffset = currentHeight;

            timer += Time.deltaTime;
            yield return null;
        }
        meshAgent.baseOffset = startHeight + newHeight;

        heightCheckRoutine = null;
    }
    /// <summary>
    /// Triggers a coroutine that rolls the selected transform along its local z axis
    /// </summary>
    /// <param name="isClockwise">Whether to rotate clockwise or counter clockwise</param>
    public void BarrelRoll(int direction, float force)
    {
        rollRoutine ??= StartCoroutine(Roll(rollTime, direction, force));
    }
    private IEnumerator Roll(float rollTime, int direction, float force)
    {
        float timer = 0f;
        rb.linearDamping = rollResistance;

        // Disable the navmesh agent, then apply force.
        meshAgent.enabled = false;
        rb.AddForce(Vector3.right * direction * force, ForceMode.Impulse);
        
        while (timer < rollTime)
        {
            modelParts.rotation = Quaternion.Euler(modelParts.eulerAngles.x, modelParts.eulerAngles.y, Mathf.Lerp(0f, 360f * direction, timer / rollTime));

            timer += Time.deltaTime;
            yield return null;
        }
        modelParts.rotation = Quaternion.Euler(modelParts.eulerAngles.x, modelParts.eulerAngles.y, 0f);
        rb.linearDamping = 0f;
        
        // Reset the velocity, then reenable the navmesh agent
        rb.linearVelocity = new Vector3();
        meshAgent.enabled = true;

        rollRoutine = null;
    }
}
