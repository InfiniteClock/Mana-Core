using System.Collections;
using System.ComponentModel;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(FlyerWeapon))]
[RequireComponent(typeof(NavMeshAgent))]
public class FlyerEnemy : Enemy
{
    public float maxHeight;
    public float minHeight;
    public float rollTime;
    public Transform rollables;

    private float agentRadius;
    private Coroutine rollRoutine;
    private Coroutine heightCheckRoutine;
    private readonly WaitForSeconds heightCheckWait = new(0.25f);
    private BehaviorGraphAgent agent;
    private BlackboardVariable<FlyerSecondaryState> fSecondState;
    private FlyerWeapon fWeapon;
    private NavMeshAgent meshAgent;

    protected override void Start()
    {
        base.Start();
        fWeapon = GetComponent<FlyerWeapon>();
        agent = GetComponent<BehaviorGraphAgent>();

        agent.GetVariable("SecondaryState", out fSecondState);
        meshAgent = GetComponent<NavMeshAgent>();
        agentRadius = meshAgent.radius;
    }
    public void ChangeHeight(float additionalHeight)
    {
        // Check that new height is within range
        if (meshAgent.height + additionalHeight > maxHeight)
            additionalHeight = maxHeight;
        if (meshAgent.height + additionalHeight < minHeight)
            additionalHeight = minHeight;

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
        float startHeight = meshAgent.height;
        float timer = 0f;
        float timeToHeight = newHeight / speed;
        
        while (timer < timeToHeight)
        {
            float currentHeight = Mathf.Lerp(startHeight, startHeight + newHeight, timer / timeToHeight);
            meshAgent.height = currentHeight;
            meshAgent.baseOffset = currentHeight;

            timer += Time.deltaTime;
            yield return null;
        }
        meshAgent.height = newHeight;
        meshAgent.baseOffset = newHeight;

        heightCheckRoutine = null;
    }
    /// <summary>
    /// Triggers a coroutine that rolls the selected transform along its local z axis
    /// </summary>
    /// <param name="isClockwise">Whether to rotate clockwise or counter clockwise</param>
    public void BarrelRoll(int direction)
    {
        rollRoutine ??= StartCoroutine(Roll(rollTime, direction));
    }
    private IEnumerator Roll(float rollTime, int direction)
    {
        float timer = 0f;

        while (timer < rollTime)
        {
            rollables.rotation = Quaternion.Euler(rollables.eulerAngles.x, rollables.eulerAngles.y, Mathf.Lerp(0f, 360f * direction, timer / rollTime));

            timer += Time.deltaTime;
            yield return null;
        }
        rollables.rotation = Quaternion.Euler(rollables.eulerAngles.x, rollables.eulerAngles.y, 0f);
        rollRoutine = null;
    }
}
