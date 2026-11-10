using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Target Detection", story: "Update [TargetDetector] Update [TargetDetected] and Assign [Target]", category: "Action", id: "c0c412e7b3d60b0e1a818a851a5bcfac")]
public partial class TargetDetectionAction : Action
{
    [SerializeReference] public BlackboardVariable<TargetDetector> TargetDetector;
    [SerializeReference] public BlackboardVariable<bool> TargetDetected;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnUpdate()
    {
        Target.Value = TargetDetector.Value.UpdateDetectTarget();
        TargetDetected.Value = TargetDetector.Value.UpdateDetectTargetTimers(Target.Value != null);
        return Target.Value != null ? Status.Success : Status.Failure;
    }
}

