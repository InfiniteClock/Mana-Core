using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LookAtTarget", story: "Looks towards [CurrentTarget] while [SeeTarget] using [Enemy]", category: "Action", id: "3a82dab4b9ee4e3b5dedfe53876967dc")]
public partial class LookAtTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;
    [SerializeReference] public BlackboardVariable<bool> SeeTarget;
    [SerializeReference] public BlackboardVariable<Enemy> Enemy;

    protected override Status OnUpdate()
    {
        if (SeeTarget.Value)
            Enemy.Value.LookAtTarget(CurrentTarget.Value.transform);
        else
            Enemy.Value.LookAtTarget(null);

        return Status.Success;
    }
    
}

