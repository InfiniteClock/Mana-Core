using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Location", story: "Set [Self] [Position] to [float] from [CurrentTarget]", category: "Action", id: "7083d483ecf470240d2068c9e2e08229")]
public partial class SetLocationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    [SerializeReference] public BlackboardVariable<float> Float;
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;


    protected override Status OnUpdate()
    {
        if (CurrentTarget.Value == null || Self.Value == null || Position.Value == null) return Status.Failure;

        Vector3 selfPos = Self.Value.transform.position;
        Vector3 direction = (selfPos - CurrentTarget.Value.transform.position).normalized;
        Position.Value = CurrentTarget.Value.transform.position + direction * Float.Value;
        return Status.Success;
    }

}

