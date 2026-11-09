using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Range Detector", story: "Update [Detector] and assign [Target]", category: "Action", id: "711c58e726afbe751b4bb3deaba2cc04")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<TargetDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;


    protected override Status OnUpdate()
    {
        Target.Value = Detector.Value.UpdateDetectTarget();
        return Target.Value != null ? Status.Success : Status.Failure;
    }

}

