using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PickSecondaryState", story: "Select a random [SecondaryState]", category: "Action", id: "ec62681d42922e745b53c949e46da4bd")]
public partial class PickSecondaryStateAction : Action
{
    [SerializeReference] public BlackboardVariable<FodderSecondaryState> SecondaryState;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

