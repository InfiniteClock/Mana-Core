using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FlyerReposition", story: "[Self] moves [distance] to [EvadeDestination] using [FlyerEnemy]", category: "Action", id: "62b0aaa160f6b9882cf77c22d86844dc")]
public partial class FlyerRepositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<Vector3> EvadeDestination;
    [SerializeReference] public BlackboardVariable<FlyerEnemy> FlyerEnemy;

    private NavMeshAgent meshAgent;
    protected override Status OnStart()
    {
        meshAgent = Self.Value.GetComponent<NavMeshAgent>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (meshAgent == null) return Status.Failure;

        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized * Distance.Value;
        EvadeDestination.Value = Self.Value.transform.position + Self.Value.transform.right * randomDir.x;
        FlyerEnemy.Value.ChangeHeight(randomDir.y);

        return Status.Success;
    }

}

