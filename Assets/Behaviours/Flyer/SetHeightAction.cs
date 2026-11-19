using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetHeight", story: "Set [height] of [FlyerEnemy]", category: "Action", id: "4bf3e6e50fcfb4df9bb46e0ccb23c547")]
public partial class SetHeightAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Height;
    [SerializeReference] public BlackboardVariable<FlyerEnemy> FlyerEnemy;
    private NavMeshAgent meshAgent;
    protected override Status OnStart()
    {
        if (FlyerEnemy == null) return Status.Failure;

        meshAgent = FlyerEnemy.Value.GetComponent<NavMeshAgent>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (FlyerEnemy == null) return Status.Failure;

        FlyerEnemy.Value.ChangeHeight(Height.Value - meshAgent.baseOffset);
        return Status.Success;
    }

}

